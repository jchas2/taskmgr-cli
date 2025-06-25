namespace Task.Manager.System.Process;

public interface IProcessor
{
    event EventHandler<ProcessorEventArgs> ProcessorUpdated;
    public int GhostProcessCount { get; }
    public int ProcessCount { get; }
    public void Run();
    public void Stop();
    public int ThreadCount { get; }

}