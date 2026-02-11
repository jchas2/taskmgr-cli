using System.Reflection;
using Task.Manager.System.Process;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Tests.Process;

public sealed class ProcessInfoTests
{
    [Fact]
    public void ProcessInfo_Canary_Test()
    {
        // If this fails, review all tests in this class                                                                                   
        // and update the expected count after adding tests                                                                                
        const int ExpectedPropertyCount = 19;

        int actualCount = typeof(ProcessInfo).GetProperties(
            BindingFlags.Public | BindingFlags.Instance).Length;

        Assert.Equal(ExpectedPropertyCount, actualCount);
    }
    
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
