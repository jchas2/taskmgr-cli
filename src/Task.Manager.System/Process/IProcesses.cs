namespace Task.Manager.System.Process;

public interface IProcesses
{
    ProcessInfo[] GetAll();
    bool GetProcessTimes(in int pid, ref ProcessTimeInfo ptInfo);
    public int GhostProcessCount { get; }
    public int ProcessCount { get; }
    public int ThreadCount { get; }

}