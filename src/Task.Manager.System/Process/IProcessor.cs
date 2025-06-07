namespace Task.Manager.System.Process;

public interface IProcessor
{
    ProcessInfo[] GetAll();
    public int GhostProcessCount { get; }
    public int ProcessCount { get; }
    public void Run();
    public void Stop();
    public int ThreadCount { get; }

}