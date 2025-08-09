using System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public partial class Processor : IProcessor
{
    private const int InitialBufferSize = 512;
    
    private readonly ISystemInfo _systemInfo;
    private SystemStatistics _systemStatistics;
    private List<ProcessInfo> _allProcesses;
    private List<ProcessInfo> _allProcessesCopy;
    private readonly Dictionary<int, ProcessInfo> _processMap;
    private Thread? _workerThread;
    private Thread? _monitorThread;
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
        _allProcesses = new List<ProcessInfo>(InitialBufferSize);
        _allProcessesCopy = new List<ProcessInfo>(InitialBufferSize);
        _processMap = new Dictionary<int, ProcessInfo>();
        _lock = new Lock();
        _isWindows = OperatingSystem.IsWindows();
    }
    
    private bool GetProcessTimes(in int pid, ref ProcessTimeInfo ptInfo)
    {
        try {
            if (!ProcessUtils.TryGetProcessByPid(pid, out SysDiag::Process? process)) {
                return false;
            }
            
            TryMapProcessTimeInfo(process!, ref ptInfo);
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
        
        _monitorThread = new Thread(() => RunMonitorInternal(_cancellationTokenSource.Token));
        _monitorThread.Start();
    }

    private void RunInternal(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) {
            SysDiag::Process[] procs = SysDiag::Process.GetProcesses();

            int delta = 0;
            int index = 0;

            _allProcesses.Clear();
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

                if (!_processMap.TryGetValue(procs[index].Id, out ProcessInfo? procInfo)) {
                    procInfo = new ProcessInfo();

                    if (!TryMapProcessInfo(procs[index], procInfo)) {
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

                    if (!TryMapProcessInfo(procs[index], procInfo)) {
                        delta++;
                        continue;
                    }

                    _processMap[procInfo.Pid] = procInfo;
                }

                procInfo.UsedMemory = procs[index].WorkingSet64;
                procInfo.DiskOperations = ProcessUtils.GetProcessIoOperations(procs[index]) ;
                procInfo.DiskUsage = 0;
                procInfo.CpuTimePercent = 0.0;
                procInfo.CpuUserTimePercent = 0.0;
                procInfo.CpuKernelTimePercent = 0.0;
                procInfo.PrevCpuKernelTime = prevProcTimes.KernelTime;
                procInfo.PrevCpuUserTime = prevProcTimes.UserTime;
                procInfo.CurrCpuKernelTime = 0;
                procInfo.CurrCpuUserTime = 0;

                _allProcesses.Add(procInfo);
                _processCount++;
                _threadCount += procs[index].Threads.Count;
            }
            
            if (cancellationToken.IsCancellationRequested) {
                return;
            }

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
            _systemStatistics.DiskUsage = 0;

            _systemInfo.GetSystemInfo(ref _systemStatistics);
            _systemInfo.GetSystemMemory(ref _systemStatistics);

            long totalSysTime = sysTimesDeltas.Kernel + sysTimesDeltas.User;
            ProcessTimeInfo currProcTimes = new();

            for (int i = 0; i < _allProcesses.Count; i++) {
                
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
                
                ulong prevDiskOperations = _allProcesses[i].DiskOperations;
                
                _allProcesses[i].DiskOperations = ProcessUtils.GetProcessIoOperations(_allProcesses[i].Pid);
                
                ulong currDiskOperations = _allProcesses[i].DiskOperations;

                _allProcesses[i].DiskUsage = 
                    (long)((double)(currDiskOperations - prevDiskOperations) * (1000.0 / (double)UpdateTimeInMs));
                
                _systemStatistics.DiskUsage += _allProcesses[i].DiskUsage;
            }
            
            _ghostProcessCount = delta;
            
            _systemStatistics.CpuPercentIdleTime = 100.0 - (_systemStatistics.CpuPercentUserTime + _systemStatistics.CpuPercentKernelTime);
            _systemStatistics.ProcessCount = _processCount;
            _systemStatistics.ThreadCount = _threadCount;
            _systemStatistics.GhostProcessCount = _ghostProcessCount;

            lock (_lock) {
                _allProcessesCopy.Clear();

                for (int i = 0; i < _allProcesses.Count; i++) {
                    _allProcessesCopy.Add(new ProcessInfo(_allProcesses[i]));
                }
            }
        }
    }

    private void RunMonitorInternal(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) {
            lock (_lock) {
                ProcessorEventArgs eventArgs = new(_allProcessesCopy, _systemStatistics);
                ProcessorUpdated?.Invoke(this, eventArgs);
            }
            
            Thread.Sleep(UpdateTimeInMs);
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();

        List<Thread?> threads = new() { _workerThread, _monitorThread };

        for (int i = 0; i < threads.Count; i++) {
            Thread? thread = threads[i];

            if (thread != null) {
                
                while (thread.IsAlive) {
                    Thread.Sleep(100);
                }
                
                thread = null;
            }
        }
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
    private bool TryMapProcessInfo(SysDiag::Process process, ProcessInfo procInfo)
    {
        try {
            procInfo.Pid = process.Id;
            procInfo.ParentPid = 0;
            procInfo.ExeName = process.ProcessName;
            procInfo.FileDescription = ProcessUtils.GetProcessProductName(process);
            procInfo.UserName = ProcessUtils.GetProcessUserName(process);
            procInfo.CmdLine = ProcessUtils.GetProcessCommandLine(process);
            procInfo.StartTime = process.StartTime;
            procInfo.ThreadCount = process.Threads.Count;
            procInfo.HandleCount = ProcessUtils.GetHandleCount(process);
            procInfo.BasePriority = process.BasePriority;
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
