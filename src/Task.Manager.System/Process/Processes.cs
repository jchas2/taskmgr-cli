using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public class Processes
{
    private const int InitialBufSize = 512;
    private List<ProcessInfo> _allProcesses;

    public Processes()
    {
        _allProcesses = new List<ProcessInfo>(capacity: InitialBufSize);
    }

    public IList<ProcessInfo> GetAll()
    {
        _allProcesses.Clear();
        var procs = SysDiag::Process.GetProcesses();

        for (int i = 0; i < procs.Length; i++) {

            if (procs[i].Id == 0) {
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
