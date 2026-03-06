namespace Task.Manager.System.Process;

public interface IProcessService
{
    IEnumerable<ProcessInfo> GetProcesses();
    ProcessInfo? GetProcessById(int pid);
}
