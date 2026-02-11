using System.Reflection;
using Task.Manager.System.Process;

namespace Task.Manager.System.Tests.Process;

public sealed class ThreadInfoTests
{
    [Fact]
    public void ThreadInfo_Canary_Test()
    {
        // If this fails, review all tests in this class                                                                                   
        // and update the expected count after adding tests                                                                                
        const int ExpectedPropertyCount = 8;

        int actualCount = typeof(ThreadInfo).GetProperties(
            BindingFlags.Public | BindingFlags.Instance).Length;

        Assert.Equal(ExpectedPropertyCount, actualCount);
    }
    
    [Fact]
    public void ThreadInfo_Write_Read_Test()
    {
        ThreadInfo threadInfo = new() {
            ThreadId = 1,
            CpuKernelTime = TimeSpan.MaxValue,
            CpuTotalTime = TimeSpan.MaxValue,
            CpuUserTime = TimeSpan.MaxValue,
            Priority = 8,
            Reason = "ExecutionDelay",
            StartAddress = long.MaxValue,
            ThreadState = "Wait"
        };
        
        Assert.Equal(1, threadInfo.ThreadId);
        Assert.Equal(TimeSpan.MaxValue, threadInfo.CpuKernelTime);
        Assert.Equal(TimeSpan.MaxValue, threadInfo.CpuTotalTime);
        Assert.Equal(TimeSpan.MaxValue, threadInfo.CpuUserTime);
        Assert.Equal(8, threadInfo.Priority);
        Assert.Equal("ExecutionDelay", threadInfo.Reason);
        Assert.Equal(long.MaxValue, threadInfo.StartAddress);
        Assert.Equal("Wait", threadInfo.ThreadState);
    }
}
 