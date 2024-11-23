using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public class Processes : IProcesses
{
    private const int InitialBufSize = 512;
    private ProcessInfo[] _allProcesses;
    private bool _isWindows = false;
    
    public Processes()
    {
        _allProcesses = new ProcessInfo[InitialBufSize];
        _isWindows = OperatingSystem.IsWindows();
    }

    public ProcessInfo[] GetAll()
    {
        Array.Clear(_allProcesses, 0, _allProcesses.Length);
        SysDiag::Process[] procs = SysDiag::Process.GetProcesses();

        for (int i = 0; i < procs.Length; i++) {
#if __WIN32__
            /* On Windows, ignore the system "idle" process auto assigned to Pid 0. */
            if (_isWindows && procs[i].Id == 0) {
                continue;
            }
#endif
            /* Skip any process that generates an "Access Denied" Exception. */
            if (null == TryGetProcessHandle(procs[i])) {
                continue;
            }
            
            var previousTimes = new ProcessTimeInfo();
            MapProcessTimes(procs[i], ref previousTimes);
            
            var procInfo = new ProcessInfo { 
                Pid = procs[i].Id,
                Handle = procs[i].Handle,
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

            if (i == _allProcesses.Length) {
                Array.Resize(ref _allProcesses, _allProcesses.Length * 2);
            }
            
            _allProcesses[i] =procInfo;
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
