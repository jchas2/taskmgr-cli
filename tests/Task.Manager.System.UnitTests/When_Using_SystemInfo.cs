namespace Task.Manager.System.UnitTests;

public sealed class When_Using_SystemInfo
{
    [Fact]
    public void Should_Get_Cpu_Times()
    {
        var systemTimes = new SystemTimes();
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
}

