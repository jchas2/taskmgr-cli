using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public class Processes
{
    private const int InitialBufSize = 512;
    private List<ProcessInfo> _allProcesses;
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
            // On Windows, ignore the system "idle" process auto assigned to Pid 0.
            if (_isWindows && procs[i].Id == 0) {
                continue;
            }

            var procInfo = new ProcessInfo
            {
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
                DiskOperations = 0,
                ProcessorTime = procs[i].TotalProcessorTime,
                ProcessorUserTime = procs[i].UserProcessorTime,
                ProcessorKernelTime = procs[i].PrivilegedProcessorTime
            };

            _allProcesses.Add(procInfo);
        }

        return _allProcesses;
    }
}
