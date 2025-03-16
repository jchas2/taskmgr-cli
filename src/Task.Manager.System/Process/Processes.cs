using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using Task.Manager.Interop.Mach;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public partial class Processes : IProcesses
{
    private readonly ISystemInfo _systemInfo;

    private const int INIT_BUFF_SIZE = 512;
    public const int UPDATE_TIME_MS = 2000;
    
    private ProcessInfo[] _allProcesses;
    private Dictionary<int, ProcessInfo> _processMap;
    private readonly bool _isWindows = false;
    private int _processCount = 0;
    private int _ghostProcessCount = 0;
    private int _threadCount = 0;
    
    public Processes()
    {
        _systemInfo = new SystemInfo();
        _allProcesses = new ProcessInfo[INIT_BUFF_SIZE];
        _processMap = new Dictionary<int, ProcessInfo>();
        _isWindows = OperatingSystem.IsWindows();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ProcessInfo CreateProcInfo(SysDiag::Process process) =>
        new ProcessInfo { 
            Pid = process.Id,
            ParentPid = 0,
            ExeName = process.ProcessName,
            FileDescription = GetProcessProductName(process),
            UserName = GetProcessUserName(process),
            CmdLine = GetProcessCommandLine(process),
            StartTime = process.StartTime
        };
    
     public ProcessInfo[] GetAll()
     {
        SysDiag::Process[] procs = SysDiag::Process.GetProcesses();
        Array.Clear(_allProcesses);

        int delta = 0;
        int index = 0;

        _processCount = 0;
        _threadCount = 0;

        for (index = 0; index < procs.Length; index++) {
#if __WIN32__
            /* On Windows, ignore the system "idle" process auto assigned to Pid 0. */
            if (_isWindows && procs[index].Id == 0) {
                delta++;
                continue;
            }
#endif
             /* Skip any process that generates an "Access Denied" Exception. */
            if (null == TryGetProcessHandle(procs[index])) {
                delta++;
                continue;
            }
            
            var prevProcTimes = new ProcessTimeInfo();
            MapProcessTimes(procs[index], ref prevProcTimes);

            if (false == _processMap.TryGetValue(procs[index].Id, out ProcessInfo procInfo)) {
                procInfo = CreateProcInfo(procs[index]);
                _processMap.Add(procInfo.Pid, procInfo);
            }

            /* Pid has been reallocated to new process. */
            if (false == procInfo.StartTime.Equals(procs[index].StartTime)) {
                UpdateProcInfo(ref procInfo, procs[index]);
                _processMap[procInfo.Pid] = procInfo;
            }

            procInfo.ThreadCount = procs[index].Threads.Count;
            procInfo.BasePriority = procs[index].BasePriority;
            procInfo.ParentPid = 0;
            procInfo.UsedMemory = procs[index].WorkingSet64;
            procInfo.DiskOperations = 0;
            procInfo.DiskUsage = 0;
            procInfo.CpuTimePercent = 0.0;
            procInfo.CpuUserTimePercent = 0.0; 
            procInfo.CpuKernelTimePercent = 0.0;
            procInfo.PrevCpuKernelTime = prevProcTimes.KernelTime;
            procInfo.PrevCpuUserTime = prevProcTimes.UserTime;
            procInfo.CurrCpuKernelTime = 0;
            procInfo.CurrCpuUserTime = 0;
           
            if (index == _allProcesses.Length) {
                Array.Resize(ref _allProcesses, _allProcesses.Length * 2);
            }
           
            _allProcesses[index - delta] = procInfo;
            _processCount++;
            _threadCount += procs[index].Threads.Count;
        }
        
        Array.Resize(ref _allProcesses, index - delta);
        
        GetSystemTimes(out SystemTimes prevSysTimes);
        Thread.Sleep(UPDATE_TIME_MS);
        GetSystemTimes(out SystemTimes currSysTimes);
        
        var sysTimesDeltas = new SystemTimes {
            Idle = currSysTimes.Idle - prevSysTimes.Idle,
            Kernel = currSysTimes.Kernel - prevSysTimes.Kernel,
            User = currSysTimes.User - prevSysTimes.User
        };

        long totalSysTime = sysTimesDeltas.Kernel + sysTimesDeltas.User;
        var currProcTimes = new ProcessTimeInfo();
        
        for (int i = 0; i < _allProcesses.Length; i++) {
            currProcTimes.Clear();
            GetProcessTimes(_allProcesses[i].Pid, ref currProcTimes);

            _allProcesses[i].CurrCpuKernelTime = currProcTimes.KernelTime;
            _allProcesses[i].CurrCpuUserTime = currProcTimes.UserTime;
            
            long procKernelDiff = _allProcesses[i].CurrCpuKernelTime - _allProcesses[i].PrevCpuKernelTime;
            long procUserDiff = _allProcesses[i].CurrCpuUserTime - _allProcesses[i].PrevCpuUserTime;
            long totalProc = procKernelDiff + procUserDiff;

            if (totalSysTime == 0) {
                continue;
            }

            _allProcesses[i].CpuTimePercent = 100 * (double)totalProc / totalSysTime;
            _allProcesses[i].CpuKernelTimePercent = 100 * (double)procKernelDiff / totalSysTime;
            _allProcesses[i].CpuUserTimePercent = 100 * (double)procUserDiff / totalSysTime;
        }

        _ghostProcessCount = delta;
        
        return _allProcesses;
    }

    public bool GetProcessTimes(in int pid, ref ProcessTimeInfo ptInfo)
    {
        try {
            var proc = SysDiag::Process.GetProcessById(pid);
            MapProcessTimes(proc, ref ptInfo);
            return true;
        }
        catch (ArgumentException) {
            return false;
        }
        // TODO: Catch all here for a graceful exit of the process.
        catch (Exception) {
            return false;
        }
    }
    
    private void GetSystemTimes(out SystemTimes systemTimes)
    {
        systemTimes = new SystemTimes();

        if (false == _systemInfo.GetCpuTimes(ref systemTimes)) {
            systemTimes.Idle = 0;
            systemTimes.Kernel = 0;
            systemTimes.User = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MapProcessTimes(SysDiag::Process proc, ref ProcessTimeInfo ptInfo)
    {
        ptInfo.DiskOperations = 0;

        /*
         * On MacOS, these properties can still throw an Exception when running as root.
         */
        try {
#if __WIN32__             
            ptInfo.KernelTime = proc.PrivilegedProcessorTime.Ticks;
            ptInfo.UserTime = proc.UserProcessorTime.Ticks;
#endif
#if __APPLE__
            /*
             NOTE: The calls to host_statistics64() for System CPU don't align
             with the framework calls below. To align the tick results * 1000. 
            */
            ptInfo.KernelTime = proc.PrivilegedProcessorTime.Ticks * 1000;
            ptInfo.UserTime = proc.UserProcessorTime.Ticks * 1000;
#endif            
        }
#pragma warning disable CS0168 // The variable is declared but never used
        catch (Exception e) {
#if DEBUG
            SysDiag::Debug.WriteLine($"Failed PrivilegedProcessorTime() {proc.ProcessName} {proc.Id} with {e.Message}");
#endif
        }
#pragma warning restore CS0168 // The variable is declared but never used
    }

    public int GhostProcessCount => _ghostProcessCount;
    public int ProcessCount => _processCount;
    public int ThreadCount => _threadCount;
    
    private IntPtr? TryGetProcessHandle(SysDiag::Process proc)
    {
        try {
            /*
             * On Windows (with elevated access) this call can still throw an
             * "Access denied" Exception. This is usually for the "system"
             * process assigned to Pid 4.
             */
            return proc.Handle;
        }
#pragma warning disable CS0168 // The variable is declared but never used
        catch (Exception e) {
#if DEBUG
            SysDiag::Debug.WriteLine($"Failed Handle() {proc.ProcessName} {proc.Id} with {e.Message}");
#endif
            return null;
        }
#pragma warning restore CS0168 // The variable is declared but never used
    }

    private void UpdateProcInfo(ref ProcessInfo procInfo, SysDiag::Process process)
    {
        procInfo.ParentPid = 0;
        procInfo.ExeName = process.ProcessName;
        procInfo.FileDescription = GetProcessProductName(process);
        procInfo.UserName = GetProcessUserName(process);
        procInfo.CmdLine = GetProcessCommandLine(process);
        procInfo.StartTime = process.StartTime;
    }
}
