namespace Task.Manager.System.Process;

public interface IProcessService
{
    ProcessInfo[] GetProcesses();
    ProcessInfo? GetProcessById(int pid);
}