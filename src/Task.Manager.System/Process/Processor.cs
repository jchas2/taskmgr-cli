using System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public partial class Processor : IProcessor
{
    private const int InitialBufferSize = 512;
    
    private readonly ISystemInfo systemInfo;
    private SystemStatistics systemStatistics;
    private readonly List<ProcessInfo> allProcesses;
    private readonly List<ProcessInfo> allProcessesCopy;
    private readonly Dictionary<int, ProcessInfo> processMap;
    private Thread? workerThread;
    private Thread? monitorThread;
    private CancellationTokenSource? cancellationTokenSource;
    private readonly Lock @lock;
    private readonly bool isWindows = false;
    private int processCount = 0;
    private int ghostProcessCount = 0;
    private int threadCount = 0;
    
    public const int UpdateTimeInMs = 1500;

    public event EventHandler<ProcessorEventArgs>? ProcessorUpdated;
    
    public Processor()
    {
        systemInfo = new SystemInfo();
        systemStatistics = new SystemStatistics();
        allProcesses = new List<ProcessInfo>(InitialBufferSize);
        allProcessesCopy = new List<ProcessInfo>(InitialBufferSize);
        processMap = new Dictionary<int, ProcessInfo>();
        @lock = new Lock();
        isWindows = OperatingSystem.IsWindows();
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

        if (!systemInfo.GetCpuTimes(ref systemTimes)) {
            systemTimes.Idle = 0;
            systemTimes.Kernel = 0;
            systemTimes.User = 0;
        }
    }
    
    public int GhostProcessCount => ghostProcessCount;
    
    public int ProcessCount => processCount;

    public void Run()
    {
        cancellationTokenSource = new CancellationTokenSource();

        workerThread = new Thread(() => RunInternal(cancellationTokenSource.Token));
        workerThread.Start();
        
        monitorThread = new Thread(() => RunMonitorInternal(cancellationTokenSource.Token));
        monitorThread.Start();
    }

    private void RunInternal(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) {
            SysDiag::Process[] procs = SysDiag::Process.GetProcesses();

            int delta = 0;
            int index = 0;

            allProcesses.Clear();
            processCount = 0;
            threadCount = 0;

            for (index = 0; index < procs.Length; index++) {
                
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }
#if __WIN32__
                /* On Windows, ignore the system "idle" process auto assigned to Pid 0. */
                if (isWindows && procs[index].Id == 0) {
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

                if (!processMap.TryGetValue(procs[index].Id, out ProcessInfo? procInfo)) {
                    procInfo = new ProcessInfo();

                    if (!TryMapProcessInfo(procs[index], procInfo)) {
                        delta++;
                        continue;
                    }

                    processMap.Add(procInfo.Pid, procInfo);
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

                    processMap[procInfo.Pid] = procInfo;
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

                allProcesses.Add(procInfo);
                processCount++;
                threadCount += procs[index].Threads.Count;
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

            systemStatistics.CpuPercentIdleTime = 0.0;
            systemStatistics.CpuPercentUserTime = 0.0;
            systemStatistics.CpuPercentKernelTime = 0.0;
            systemStatistics.DiskUsage = 0;

            systemInfo.GetSystemInfo(ref systemStatistics);
            systemInfo.GetSystemMemory(ref systemStatistics);

            long totalSysTime = sysTimesDeltas.Kernel + sysTimesDeltas.User;
            ProcessTimeInfo currProcTimes = new();

            for (int i = 0; i < allProcesses.Count; i++) {
                
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }

                currProcTimes.Clear();

                if (!GetProcessTimes(allProcesses[i].Pid, ref currProcTimes)) {
                    continue;
                }

                allProcesses[i].CurrCpuKernelTime = currProcTimes.KernelTime;
                allProcesses[i].CurrCpuUserTime = currProcTimes.UserTime;

                long procKernelDiff = allProcesses[i].CurrCpuKernelTime - allProcesses[i].PrevCpuKernelTime;
                long procUserDiff = allProcesses[i].CurrCpuUserTime - allProcesses[i].PrevCpuUserTime;
                long totalProc = procKernelDiff + procUserDiff;

                if (totalSysTime == 0) {
                    continue;
                }

                allProcesses[i].CpuTimePercent = 100 * (double)totalProc / totalSysTime;
                allProcesses[i].CpuKernelTimePercent = 100 * (double)procKernelDiff / totalSysTime;
                allProcesses[i].CpuUserTimePercent = 100 * (double)procUserDiff / totalSysTime;
                
                systemStatistics.CpuPercentUserTime += allProcesses[i].CpuUserTimePercent;
                systemStatistics.CpuPercentKernelTime += allProcesses[i].CpuKernelTimePercent;
                
                ulong prevDiskOperations = allProcesses[i].DiskOperations;
                
                allProcesses[i].DiskOperations = ProcessUtils.GetProcessIoOperations(allProcesses[i].Pid);
                
                ulong currDiskOperations = allProcesses[i].DiskOperations;

                allProcesses[i].DiskUsage = 
                    (long)((double)(currDiskOperations - prevDiskOperations) * (1000.0 / (double)UpdateTimeInMs));
                
                systemStatistics.DiskUsage += allProcesses[i].DiskUsage;
            }
            
            ghostProcessCount = delta;
            
            systemStatistics.CpuPercentIdleTime = 100.0 - (systemStatistics.CpuPercentUserTime + systemStatistics.CpuPercentKernelTime);
            systemStatistics.ProcessCount = processCount;
            systemStatistics.ThreadCount = threadCount;
            systemStatistics.GhostProcessCount = ghostProcessCount;

            lock (@lock) {
                allProcessesCopy.Clear();

                for (int i = 0; i < allProcesses.Count; i++) {
                    allProcessesCopy.Add(new ProcessInfo(allProcesses[i]));
                }
            }
        }
    }

    private void RunMonitorInternal(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) {
            lock (@lock) {
                ProcessorEventArgs eventArgs = new(allProcessesCopy, systemStatistics);
                ProcessorUpdated?.Invoke(this, eventArgs);
            }
            
            Thread.Sleep(UpdateTimeInMs);
        }
    }

    public void Stop()
    {
        cancellationTokenSource?.Cancel();

        List<Thread?> threads = new() { workerThread, monitorThread };

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

    public int ThreadCount => threadCount;
    
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
