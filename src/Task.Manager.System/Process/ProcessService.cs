using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public partial class ProcessService : IProcessService
{
    public ProcessInfo[] GetProcesses()
    {
        SysDiag::Process[] processes = SysDiag::Process.GetProcesses();
        ProcessInfo[] processInfos = new ProcessInfo[processes.Length]; 

        for (int i = 0; i < processes.Length; i++) {
            processInfos[i] = new ProcessInfo(processes[i]);
        }

        return processInfos;
    }

    public ProcessInfo? GetProcessById(int pid) =>
        ProcessUtils.TryGetProcessByPid(pid, out SysDiag::Process? process)
            ? new ProcessInfo(process!)
            : null;
}
