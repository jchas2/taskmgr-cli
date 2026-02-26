using System.Reflection;
using Task.Manager.System.Process;
using Task.Manager.Tests.Common;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Tests.Process;

public sealed class ProcessInfoTests
{
    [Fact]
    public void ProcessInfo_Canary_Test() =>
        Assert.Equal(19, CanaryTestHelper.GetPropertyCount<ProcessInfo>());

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
