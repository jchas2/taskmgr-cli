using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public partial class Processes : IProcesses
{
    private readonly ISystemInfo _systemInfo;

    private const int INIT_BUFF_SIZE = 512;
    private const int UPDATE_TIME_MS = 1000;
    
    private ProcessInfo[] _allProcesses;
    private readonly bool _isWindows = false;
    
    public Processes()
    {
        _systemInfo = new SystemInfo();
        _allProcesses = new ProcessInfo[INIT_BUFF_SIZE];
        _isWindows = OperatingSystem.IsWindows();
    }

    public ProcessInfo[] GetAll()
    {
        Array.Clear(_allProcesses, 0, _allProcesses.Length);
        SysDiag::Process[] procs = SysDiag::Process.GetProcesses();
        int delta = 0;

        for (int index = 0; index < procs.Length; index++) {
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
            
            var previousTimes = new ProcessTimeInfo();
            MapProcessTimes(procs[index], ref previousTimes);
            
            var procInfo = new ProcessInfo { 
                Pid = procs[index].Id,
                ThreadCount = procs[index].Threads.Count,
                BasePriority = procs[index].BasePriority,
                ParentPid = 0,
                ExeName = procs[index].ProcessName,
                FileDescription = string.Empty,
                UserName = GetProcessUserName(procs[index]),
                CmdLine = string.Empty,
                UsedMemory = procs[index].VirtualMemorySize64,
                DiskOperations = 0,
                DiskUsage = 0,
                CpuTimePercent = 0.0,
                CpuUserTimePercent = 0.0,
                CpuKernelTimePercent = 0.0,
                PrevCpuKernelTime = previousTimes.KernelTime,
                PrevCpuUserTime = previousTimes.UserTime,
                CurrCpuKernelTime = 0,
                CurrCpuUserTime = 0
            };

            if (index == _allProcesses.Length) {
                Array.Resize(ref _allProcesses, _allProcesses.Length * 2);
            }
            
            _allProcesses[index - delta] = procInfo;
        }
        
        GetSystemTimes(out SystemTimes prevSysTimes);
        Thread.Sleep(UPDATE_TIME_MS);
        GetSystemTimes(out SystemTimes currSysTimes);
        
        var sysTimesDeltas = new SystemTimes {
            Idle = currSysTimes.Idle - prevSysTimes.Idle,
            Kernel = currSysTimes.Kernel - prevSysTimes.Kernel,
            User = currSysTimes.User - prevSysTimes.User
        };

        long totalSysTime = sysTimesDeltas.Kernel + sysTimesDeltas.User;
        
        for (int i = 0; i < procs.Length; i++) {
            var currTimes = new ProcessTimeInfo();
            GetProcessTimes(_allProcesses[i].Pid, ref currTimes);

            _allProcesses[i].CurrCpuKernelTime = currTimes.KernelTime;
            _allProcesses[i].CurrCpuUserTime = currTimes.UserTime;
            
            long procKernelDiff = _allProcesses[i].CurrCpuKernelTime - _allProcesses[i].PrevCpuKernelTime;
            long procUserDiff = _allProcesses[i].CurrCpuUserTime - _allProcesses[i].PrevCpuUserTime;
            long totalProc = procKernelDiff + procUserDiff;

            if (totalSysTime == 0) {
                continue;
            }

            _allProcesses[i].CpuTimePercent = (double)((100.0 * (double)totalProc) / (double)totalSysTime);
            _allProcesses[i].CpuKernelTimePercent = (double)((100.0 * (double)procKernelDiff) / (double)totalSysTime);
            _allProcesses[i].CpuUserTimePercent = (double)((100.0 * (double)procUserDiff) / (double)totalSysTime);
        }
        
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
        ptInfo.KernelTime = proc.PrivilegedProcessorTime.Ticks;
        ptInfo.UserTime = proc.UserProcessorTime.Ticks;
    }

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
        catch (Exception) {
            return null;
        }
    }
}
