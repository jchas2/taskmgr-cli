using Task.Manager.System.Process;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Tests.Process;

public sealed class ThreadServiceTests
{
    [Fact]
    public void Should_Return_Threads_For_Current_Process()
    {
        using var currentProcess = SysDiag::Process.GetCurrentProcess();
        var threads = new ThreadService().GetThreads(currentProcess.Id);
        Assert.True(threads.Count > 0);
    }
}