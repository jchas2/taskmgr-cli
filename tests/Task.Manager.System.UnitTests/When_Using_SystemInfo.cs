namespace Task.Manager.System.UnitTests;

public class When_Using_SystemInfo
{
    [Fact]
    public void Should_Get_Cpu_Times()
    {
        var systemTimes = new SystemTimes();
        bool result = SystemInfo.GetCpuTimes(ref systemTimes);
        Assert.True(result);
    }
}
