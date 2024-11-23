namespace Task.Manager.System.Process;

public interface IProcesses
{
    ProcessInfo[] GetAll();
    bool GetProcessTimes(in int pid, ref ProcessTimeInfo ptInfo);
}