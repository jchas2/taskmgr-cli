using System.Linq.Expressions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using Task.Manager.Interop.Mach;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public partial class Processor : IProcessor
{
    private const int InitialBufferSize = 512;
    
    private readonly ISystemInfo _systemInfo;
    private SystemStatistics _systemStatistics;
    private ProcessInfo[] _allProcesses;
    private ProcessInfo[] _allProcessesCopy;
    private readonly Dictionary<int, ProcessInfo> _processMap;
    private Thread? _workerThread;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly Lock _lock;
    private readonly bool _isWindows = false;
    private int _processCount = 0;
    private int _ghostProcessCount = 0;
    private int _threadCount = 0;
    
    public const int UpdateTimeInMs = 1500;

    public event EventHandler<ProcessorEventArgs>? ProcessorUpdated;
    
    public Processor()
    {
        _systemInfo = new SystemInfo();
        _systemStatistics = new SystemStatistics();
        _allProcesses = new ProcessInfo[InitialBufferSize];
        _allProcessesCopy = [];
        _processMap = new Dictionary<int, ProcessInfo>();
        _lock = new Lock();
        _isWindows = OperatingSystem.IsWindows();
    }
    
    private bool GetProcessTimes(in int pid, ref ProcessTimeInfo ptInfo)
    {
        try {
            SysDiag::Process proc = SysDiag::Process.GetProcessById(pid);
            TryMapProcessTimeInfo(proc, ref ptInfo);
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

        if (!_systemInfo.GetCpuTimes(ref systemTimes)) {
            systemTimes.Idle = 0;
            systemTimes.Kernel = 0;
            systemTimes.User = 0;
        }
    }
    
    public int GhostProcessCount => _ghostProcessCount;
    
    public int ProcessCount => _processCount;

    public void Run()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        _workerThread = new Thread(() => RunInternal(_cancellationTokenSource.Token));
        _workerThread.Start();
    }

    private void RunInternal(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) {
            SysDiag::Process[] procs = SysDiag::Process.GetProcesses();
            Array.Clear(_allProcesses);

            int delta = 0;
            int index = 0;

            _processCount = 0;
            _threadCount = 0;

            for (index = 0; index < procs.Length; index++) {
                
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }
#if __WIN32__
                /* On Windows, ignore the system "idle" process auto assigned to Pid 0. */
                if (_isWindows && procs[index].Id == 0) {
                    delta++;
                    continue;
                }
#endif
                /* Skip any process that generates an "Access Denied" Exception. */
                if (!TryGetProcessHandle(procs[index], out _)) {
                    delta++;
                    continue;
                }

                ProcessTimeInfo prevProcTimes = new();

                if (!TryMapProcessTimeInfo(procs[index], ref prevProcTimes)) {
                    delta++;
                    continue;
                }

                if (!_processMap.TryGetValue(procs[index].Id, out ProcessInfo procInfo)) {
                    procInfo = new ProcessInfo();

                    if (!TryMapProcessInfo(procs[index], ref procInfo)) {
                        delta++;
                        continue;
                    }

                    _processMap.Add(procInfo.Pid, procInfo);
                }

                if (!TryGetProcessStartTime(procs[index], out DateTime startTime)) {
                    delta++;
                    continue;
                }

                /* Pid has been reallocated to new process. */
                if (!procInfo.StartTime.Equals(startTime)) {

                    if (!TryMapProcessInfo(procs[index], ref procInfo)) {
                        delta++;
                        continue;
                    }

                    _processMap[procInfo.Pid] = procInfo;
                }

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
            
            if (cancellationToken.IsCancellationRequested) {
                return;
            }

            Array.Resize(ref _allProcesses, index - delta);
            
            GetSystemTimes(out SystemTimes prevSysTimes);
            Thread.Sleep(UpdateTimeInMs);
            GetSystemTimes(out SystemTimes currSysTimes);
            
            SystemTimes sysTimesDeltas = new() {
                Idle = currSysTimes.Idle - prevSysTimes.Idle,
                Kernel = currSysTimes.Kernel - prevSysTimes.Kernel,
                User = currSysTimes.User - prevSysTimes.User
            };

            _systemStatistics.CpuPercentIdleTime = 0.0;
            _systemStatistics.CpuPercentUserTime = 0.0;
            _systemStatistics.CpuPercentKernelTime = 0.0;

            _systemInfo.GetSystemInfo(ref _systemStatistics);
            _systemInfo.GetSystemMemory(ref _systemStatistics);

            long totalSysTime = sysTimesDeltas.Kernel + sysTimesDeltas.User;
            ProcessTimeInfo currProcTimes = new();

            for (int i = 0; i < _allProcesses.Length; i++) {
                
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }

                currProcTimes.Clear();

                if (!GetProcessTimes(_allProcesses[i].Pid, ref currProcTimes)) {
                    continue;
                }

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
                
                _systemStatistics.CpuPercentUserTime += _allProcesses[i].CpuUserTimePercent;
                _systemStatistics.CpuPercentKernelTime += _allProcesses[i].CpuKernelTimePercent;
            }
            
            _ghostProcessCount = delta;
            
            _systemStatistics.CpuPercentIdleTime = 100.0 - (_systemStatistics.CpuPercentUserTime + _systemStatistics.CpuPercentKernelTime);
            _systemStatistics.ProcessCount = _processCount;
            _systemStatistics.ThreadCount = _threadCount;
            _systemStatistics.GhostProcessCount = _ghostProcessCount;

            // TODO: Not needed anymore as i'm using a synchronous dispatch of event handlers. 
            // Look into async dispatch of handlers so this thread does not wait for draw operations to 
            // complete.
            lock (_lock) {
                Array.Resize(ref _allProcessesCopy, _allProcesses.Length);

                Array.Copy(
                    sourceArray: _allProcesses,
                    sourceIndex: 0,
                    destinationArray: _allProcessesCopy,
                    destinationIndex: 0,
                    length: _allProcesses.Length);
                
                // Signal listeners processes and stats updated.
                ProcessorEventArgs eventArgs = new(_allProcessesCopy, _systemStatistics);
                ProcessorUpdated?.Invoke(this, eventArgs);
            }
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        
        if (_workerThread != null) {
            while (_workerThread.IsAlive) {
                Thread.Sleep(100);
            }
        }
        
        _workerThread = null;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryMapProcessTimeInfo(SysDiag::Process proc, ref ProcessTimeInfo ptInfo)
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
            return true;
        }
#pragma warning disable CS0168 // The variable is declared but never used
        catch (Exception e) {
#if DEBUG
            SysDiag::Debug.WriteLine($"Failed PrivilegedProcessorTime() {proc.ProcessName} {proc.Id} with {e.Message}");
#endif
            return false;
        }
#pragma warning restore CS0168 // The variable is declared but never used
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryMapProcessInfo(SysDiag::Process process, ref ProcessInfo procInfo)
    {
        try {
            procInfo.Pid = process.Id;
            procInfo.ParentPid = 0;
            procInfo.ExeName = process.ProcessName;
            procInfo.FileDescription = GetProcessProductName(process);
            procInfo.UserName = GetProcessUserName(process);
            procInfo.CmdLine = GetProcessCommandLine(process);
            procInfo.StartTime = process.StartTime;
            procInfo.ThreadCount = process.Threads.Count;
            procInfo.HandleCount = process.HandleCount;
            procInfo.BasePriority = process.BasePriority;
            procInfo.ParentPid = 0;
            return true;
        }
#pragma warning disable CS0168 // The variable is declared but never used
        catch (Exception e) {
            SysDiag::Debug.WriteLine($"Failed TryMapProcessInfo() {process.ProcessName} {process.Id} with {e}");
            return false;
        }
#pragma warning restore CS0168 // The variable is declared but never used
    }

    public int ThreadCount => _threadCount;
    
    private bool TryGetProcessHandle(SysDiag::Process proc, out IntPtr? processHandle)
    {
        try {
            /*
             * On Windows (with elevated access) this call can still throw an
             * "Access denied" Exception. This is usually for the "system"
             * process assigned to Pid 4.
             */
            processHandle = proc.Handle;
            return true;
        }
#pragma warning disable CS0168 // The variable is declared but never used
        catch (Exception e) {
#if DEBUG
            SysDiag::Debug.WriteLine($"Failed Handle() {proc.ProcessName} {proc.Id} with {e.Message}");
#endif
            processHandle = null;
            return false;
        }
#pragma warning restore CS0168 // The variable is declared but never used
    }

    private bool TryGetProcessStartTime(SysDiag::Process proc, out DateTime startTime)
    {
        try {
            startTime = proc.StartTime;
            return true;
        }
        catch (Exception e) {
            SysDiag.Debug.WriteLine($"Failed StartTime() {proc.ProcessName} {proc.Id} with {e.Message}");
            startTime = DateTime.Now;
            return false;
        }
    }
}
