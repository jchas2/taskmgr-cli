using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public class Processes : IProcesses
{
    private const int InitialBufSize = 512;
    private readonly IList<ProcessInfo> _allProcesses;
    private bool _isWindows = false;
    
    public Processes()
    {
        _allProcesses = new List<ProcessInfo>(capacity: InitialBufSize);
        _isWindows = OperatingSystem.IsWindows();
    }

    public IList<ProcessInfo> GetAll()
    {
        _allProcesses.Clear();
        SysDiag::Process[] procs = SysDiag::Process.GetProcesses();

        for (int i = 0; i < procs.Length; i++) {
#if __WIN32__
            // On Windows, ignore the system "idle" process auto assigned to Pid 0.
            if (_isWindows && procs[i].Id == 0) {
                continue;
            }
#endif
            // Skip any process that generates an "Access Denied" Exception.
            if (null == TryGetSafeProcessHandle(procs[i])) {
                continue;
            }
            
            var previousTimes = new ProcessTimeInfo();
            MapProcessTimes(procs[i], ref previousTimes);
            
            var procInfo = new ProcessInfo {
                Pid = procs[i].Id,
                Handle = procs[i].SafeHandle,
                ThreadCount = procs[i].Threads.Count,
                BasePriority = procs[i].BasePriority,
                ParentPid = 0,
                ExeName = procs[i].ProcessName,
                FileDescription = string.Empty,
                UserName = string.Empty,
                CmdLine = string.Empty,
                UsedMemory = procs[i].VirtualMemorySize64,
                DiskUsage = 0,
                PreviousTimes = previousTimes,
                // DiskOperations = 0,
                // ProcessorTime = procs[i].TotalProcessorTime,
                // ProcessorUserTime = procs[i].UserProcessorTime,
                // ProcessorKernelTime = procs[i].PrivilegedProcessorTime
            };
            
            _allProcesses.Add(procInfo);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MapProcessTimes(SysDiag::Process proc, ref ProcessTimeInfo ptInfo)
    {
        ptInfo.DiskOperations = 0;
        ptInfo.KernelTime = proc.PrivilegedProcessorTime.Ticks;
        ptInfo.UserTime = proc.UserProcessorTime.Ticks;
    }

    private SafeProcessHandle? TryGetSafeProcessHandle(SysDiag::Process proc)
    {
        try {
            // On Windows (with elevated access) this call can still throw an
            // an "Access denied" Exception. This is usually for the "system"
            // process assigned to Pid 4.
            return proc.SafeHandle;
        }
        catch (Exception) {
            return null;
        }
    }
}
