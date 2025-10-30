﻿using System.Runtime.CompilerServices;
using Task.Manager.Cli.Utils;
using Task.Manager.Commands;
using Task.Manager.System;
using Task.Manager.System.Process;
using WorkerTask = System.Threading.Tasks.Task;

namespace Task.Manager.Process;

public partial class Processor : IProcessor
{
    private const int InitialBufferSize = 512;

    private readonly IProcessService processService;
    private SystemStatistics systemStatistics;
    private readonly List<ProcessorInfo> allProcessorInfos;
    private readonly List<ProcessorInfo> allProcessorInfosCopy;
    private readonly Dictionary<int, ProcessorInfo> processMap;
    private WorkerTask? workerTask;
    private WorkerTask? monitorTask;
    private CancellationTokenSource? cancellationTokenSource;
    private readonly Lock @lock;
    private readonly bool isWindows = false;
    private bool dataInitialised = false;
    private int processCount = 0;
    private int threadCount = 0;
    
    public event EventHandler<ProcessorEventArgs>? ProcessorUpdated;
    
    public Processor(IProcessService processService)
    {
        this.processService = processService;
        systemStatistics = new SystemStatistics();
        allProcessorInfos = new List<ProcessorInfo>(InitialBufferSize);
        allProcessorInfosCopy = new List<ProcessorInfo>(InitialBufferSize);
        processMap = new Dictionary<int, ProcessorInfo>();
        @lock = new Lock();
        isWindows = OperatingSystem.IsWindows();
    }
    
    private void GetSystemTimes(out SystemTimes systemTimes)
    {
        systemTimes = new SystemTimes();

        if (!SystemInfo.GetCpuTimes(ref systemTimes)) {
            systemTimes.Idle = 0;
            systemTimes.Kernel = 0;
            systemTimes.User = 0;
        }
    }
    
    public int ProcessCount => processCount;

    public void Run()
    {
        cancellationTokenSource = new CancellationTokenSource();
       
        workerTask = WorkerTask.Run(() => RunInternal(cancellationTokenSource.Token));
        monitorTask = WorkerTask.Run(() => RunMonitorInternal(cancellationTokenSource.Token));
    }

    private void RunInternal(CancellationToken cancellationToken)
    {
        TimeSpan delayInMs = new TimeSpan(0, 0, 0, 0, Constants.ProcessDelayInMilliseconds);

        while (!cancellationToken.IsCancellationRequested) {
            ProcessInfo[] processInfos = processService.GetProcesses();
            int index = 0;

            allProcessorInfos.Clear();
            processCount = 0;
            threadCount = 0;

            for (index = 0; index < processInfos.Length; index++) {
                
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }
#if __WIN32__
                // On Windows, ignore the system "idle" process auto assigned to Pid 0.
                if (isWindows && processInfos[index].Pid == 0) {
                    continue;
                }
#endif
                // Skip any process that generates an "Access Denied" Exception.
                if (!TryGetProcessHandle(processInfos[index], out _)) {
                    continue;
                }

                ProcessTimeInfo prevProcTimes = new();

                if (!TryMapProcessTimeInfo(processInfos[index], ref prevProcTimes)) {
                    continue;
                }

                if (!processMap.TryGetValue(processInfos[index].Pid, out ProcessorInfo? processorInfo)) {
                    processorInfo = new ProcessorInfo();

                    if (!TryMapProcessInfo(processInfos[index], processorInfo)) {
                        continue;
                    }

                    processMap.Add(processorInfo.Pid, processorInfo);
                }

                if (!TryGetProcessStartTime(processInfos[index], out DateTime startTime)) {
                    continue;
                }

                // Pid has been reallocated to new process.
                if (!processorInfo.StartTime.Equals(startTime)) {

                    if (!TryMapProcessInfo(processInfos[index], processorInfo)) {
                        continue;
                    }

                    processMap[processorInfo.Pid] = processorInfo;
                }

                processorInfo.UsedMemory = processInfos[index].UsedMemory;
                processorInfo.DiskOperations = processInfos[index].DiskOperations ;
                processorInfo.DiskUsage = 0;
                processorInfo.CpuTimePercent = 0.0;
                processorInfo.CpuUserTimePercent = 0.0;
                processorInfo.CpuKernelTimePercent = 0.0;
                processorInfo.PrevCpuKernelTime = prevProcTimes.KernelTime;
                processorInfo.PrevCpuUserTime = prevProcTimes.UserTime;
                processorInfo.CurrCpuKernelTime = 0;
                processorInfo.CurrCpuUserTime = 0;

                allProcessorInfos.Add(processorInfo);
                processCount++;
                threadCount += processInfos[index].ThreadCount; 
            }
            
            if (cancellationToken.IsCancellationRequested) {
                return;
            }

            GetSystemTimes(out SystemTimes prevSysTimes);
            Thread.Sleep(Constants.ProcessDelayInMilliseconds);
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

            SystemInfo.GetSystemInfo(ref systemStatistics);
            SystemInfo.GetSystemMemory(ref systemStatistics);
#if __WIN32__
            long totalSysTime = Environment.ProcessorCount * delayInMs.Ticks;
#endif              
#if __APPLE__
            long totalSysTime = sysTimesDeltas.Kernel + sysTimesDeltas.User + sysTimesDeltas.Idle;
#endif
            ProcessTimeInfo currProcTimes = new();

            for (int i = 0; i < allProcessorInfos.Count; i++) {
                
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }

                currProcTimes.Clear();

                if (!TryGetProcessTimes(allProcessorInfos[i].Pid, ref currProcTimes)) {
                    continue;
                }
                
                allProcessorInfos[i].CurrCpuKernelTime = currProcTimes.KernelTime;
                allProcessorInfos[i].CurrCpuUserTime = currProcTimes.UserTime;

                long procKernelDiff = allProcessorInfos[i].CurrCpuKernelTime - allProcessorInfos[i].PrevCpuKernelTime;
                long procUserDiff = allProcessorInfos[i].CurrCpuUserTime - allProcessorInfos[i].PrevCpuUserTime;
                long totalProc = procKernelDiff + procUserDiff;

                if (totalSysTime == 0) {
                    continue;
                }
#if __WIN32__
                // Solaris mode to be consistent with Task Manager, where 100% is full utilisation of ALL cores on the system. 
                allProcessorInfos[i].CpuTimePercent = (double)totalProc / (double)totalSysTime;
                allProcessorInfos[i].CpuKernelTimePercent = (double)procKernelDiff / (double)totalSysTime;
                allProcessorInfos[i].CpuUserTimePercent = (double)procUserDiff / (double)totalSysTime;
#endif
#if __APPLE__
                // Irix mode to be consistent with Activity Monitor, where 100% is full utilisation of a SINGLE cpu core on the system.
                allProcessorInfos[i].CpuTimePercent = Environment.ProcessorCount * (double)totalProc / (double)(delayInMs.Ticks * (long)Environment.ProcessorCount);
                allProcessorInfos[i].CpuKernelTimePercent = Environment.ProcessorCount * (double)procKernelDiff / (double)(delayInMs.Ticks * (long)Environment.ProcessorCount);
                allProcessorInfos[i].CpuUserTimePercent = Environment.ProcessorCount * (double)procUserDiff / (double)(delayInMs.Ticks * (long)Environment.ProcessorCount);
#endif
                ulong prevDiskOperations = allProcessorInfos[i].DiskOperations;
                
                allProcessorInfos[i].DiskOperations = ProcessUtils.GetProcessIoOperations(allProcessorInfos[i].Pid);
                
                ulong currDiskOperations = allProcessorInfos[i].DiskOperations;

                allProcessorInfos[i].DiskUsage = 
                    (long)((double)(currDiskOperations - prevDiskOperations) * (1000.0 / (double)Constants.ProcessDelayInMilliseconds));
                
                systemStatistics.DiskUsage += allProcessorInfos[i].DiskUsage;
            }

            systemStatistics.CpuPercentUserTime = (double)sysTimesDeltas.User / (double)totalSysTime;
            systemStatistics.CpuPercentKernelTime = (double)sysTimesDeltas.Kernel / (double)totalSysTime;
            systemStatistics.CpuPercentIdleTime = (double)sysTimesDeltas.Idle / (double)totalSysTime;
            systemStatistics.ProcessCount = processCount;
            systemStatistics.ThreadCount = threadCount;

            lock (@lock) {
                allProcessorInfosCopy.Clear();

                for (int i = 0; i < allProcessorInfos.Count; i++) {
                    allProcessorInfosCopy.Add(new ProcessorInfo(allProcessorInfos[i]));
                }
            }

            if (!dataInitialised) {
                dataInitialised = true;
            }
        }
    }

    private void RunMonitorInternal(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) {
            Thread.Sleep(Constants.ProcessDelayInMilliseconds);

            if (!dataInitialised) {
                continue;
            }
            
            lock (@lock) {
                ProcessorEventArgs eventArgs = new(allProcessorInfosCopy, systemStatistics);
                ProcessorUpdated?.Invoke(this, eventArgs);
            }
        }
    }

    public void Stop()
    {
        cancellationTokenSource?.Cancel();
        WorkerTask[] workerTasks = { workerTask!, monitorTask! };

        try {
            WorkerTask.WaitAll(workerTasks);
        }
        catch (AggregateException aggEx) {
            ExceptionHelper.HandleWaitAllException(aggEx);
        }
    }

    private bool TryGetProcessTimes(in int pid, ref ProcessTimeInfo ptInfo)
    {
        try {
            ProcessInfo? processInfo = processService.GetProcessById(pid);

            if (processInfo == null){
                return false;
            }
            
            TryMapProcessTimeInfo(processInfo, ref ptInfo);
            return true;
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex, $"Failed TryGetProcessTimes() {pid}");
            return false;
        }


    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryMapProcessTimeInfo(ProcessInfo processInfo, ref ProcessTimeInfo ptInfo)
    {
        ptInfo.DiskOperations = 0;
         // On MacOS, these properties can still throw an Exception when running as root.
        try {
            ptInfo.KernelTime = processInfo.KernelTime;
            ptInfo.UserTime = processInfo.UserTime;
            return true;
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex, $"Failed TryMapProcessTimeInfo() {processInfo.ProcessName} {processInfo.Pid}");
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryMapProcessInfo(ProcessInfo processInfo, ProcessorInfo processorInfo)
    {
        try {
            processorInfo.Pid = processInfo.Pid;
            processorInfo.ParentPid = processInfo.ParentPid;
            processorInfo.IsDaemon = processInfo.IsDaemon;
            processorInfo.ProcessName = processInfo.ProcessName;
            processorInfo.FileDescription = processInfo.FileDescription;
            processorInfo.UserName = processInfo.UserName;
            processorInfo.CmdLine = processInfo.CmdLine;
            processorInfo.StartTime = processInfo.StartTime;
            processorInfo.ThreadCount = processInfo.ThreadCount;
            processorInfo.HandleCount = processInfo.HandleCount;
            processorInfo.BasePriority = processInfo.BasePriority;
            return true;
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex, $"Failed TryMapProcessInfo() {processInfo.ProcessName} {processInfo.Pid}");
            return false;
        }
    }

    public int ThreadCount => threadCount;
    
    private bool TryGetProcessHandle(ProcessInfo processInfo, out IntPtr? processHandle)
    {
        try {
             // On Windows (with elevated access) this call can still throw an
             // "Access denied" Exception. This is usually for the "system"
             // process assigned to Pid 4.
            processHandle = processInfo.Handle;
            return true;
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex, $"Failed Handle() {processInfo.ProcessName} {processInfo.Pid}");
            processHandle = null;
            return false;
        }
    }

    private bool TryGetProcessStartTime(ProcessInfo processInfo, out DateTime startTime)
    {
        try {
            startTime = processInfo.StartTime;
            return true;
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex, $"Failed StartTime() {processInfo.ProcessName} {processInfo.Pid}");
            startTime = DateTime.Now;
            return false;
        }
    }
}
