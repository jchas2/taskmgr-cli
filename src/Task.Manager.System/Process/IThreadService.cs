namespace Task.Manager.System.Process;

public interface IThreadService
{
    List<ThreadInfo> GetThreads(int pid);
}