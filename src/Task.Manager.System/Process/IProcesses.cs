namespace Task.Manager.System.Process;

public interface IProcesses
{
    IList<ProcessInfo> GetAll();
    bool GetProcessTimes(in int pid, ref ProcessTimeInfo ptInfo);
}