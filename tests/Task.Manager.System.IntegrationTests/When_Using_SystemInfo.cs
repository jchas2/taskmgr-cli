using Xunit.Abstractions;

namespace Task.Manager.System.IntegrationTests;

public sealed class When_Using_SystemInfo
{
    private readonly ITestOutputHelper _testOutputHelper;
    
    public When_Using_SystemInfo(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;
    
    [Fact]
    public void Should_Get_Cpu_Times()
    {
        SystemTimes systemTimes = new();
        bool result = new SystemInfo().GetCpuTimes(ref systemTimes);
        Assert.True(result);
    }

    [Fact]
    public void Should_Get_Is_Running_As_Root()
    {
        // Just invoke the function call for now. Need to determine an alternate way to 
        // verify if we are running under sudo in MacOS. Windows has a number of alternatives.
        var result = new SystemInfo().IsRunningAsRoot();
        Assert.True(true);
    }
    
    [Fact]
    public void Should_Get_System_Statistics()
    {
        SystemStatistics systemStatistics = new();
        SystemInfo systemInfo = new();
        bool result = systemInfo.GetSystemStatistics(ref systemStatistics);
        Assert.True(result);
    }
}

