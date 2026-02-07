using Task.Manager.System.Process;

namespace Task.Manager.Tests.Process;

public sealed class ThreadServiceFake : IThreadService
{
    private readonly List<ThreadInfo> threadInfos = [];

    public ThreadServiceFake Add(ThreadInfo threadiInfo)
    {
        threadInfos.Add(threadiInfo);
        return this;
    }

    public List<ThreadInfo> GetThreads(int pid) => threadInfos;
}
