using Task.Manager.System.Process;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Tests.Process;

public sealed class ProcessInfoTests
{
    [Fact]
    public void Should_Construct_ProcessInfo_From_Process()
    {
        using SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        ProcessInfo processInfo = new(currentProcess);
        
        Assert.InRange(processInfo.Pid, 0, int.MaxValue);
        ProcessInfoHelpers.AssertProcessInfoProperties(processInfo);
        ProcessInfoHelpers.AssertProcessInfoProperties(currentProcess, processInfo);
    }
}
