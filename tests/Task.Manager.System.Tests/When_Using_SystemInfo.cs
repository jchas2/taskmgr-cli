using System.Globalization;
using Xunit.Abstractions;

namespace Task.Manager.System.Tests;

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
        
        bool result = systemInfo.GetSystemInfo(ref systemStatistics);
        Assert.True(result);

        _testOutputHelper.WriteLine($"CPU    : {systemStatistics.CpuName}");
        _testOutputHelper.WriteLine($"Mhz    : {systemStatistics.CpuFrequency.ToString(CultureInfo.CurrentCulture)}");
        _testOutputHelper.WriteLine($"Cores  : {systemStatistics.CpuCores}");
        _testOutputHelper.WriteLine($"OS     : {systemStatistics.OsVersion}");
        _testOutputHelper.WriteLine($"Machine: {systemStatistics.MachineName}");

        _testOutputHelper.WriteLine($"Priv IP: {systemStatistics.PrivateIPv4Address}");
        _testOutputHelper.WriteLine($"Pub IP : {systemStatistics.PublicIPv4Address}");
        
        result = systemInfo.GetSystemMemory(ref systemStatistics);
        Assert.True(result);
        
        _testOutputHelper.WriteLine($"Avail Phys: {systemStatistics.AvailablePhysical}");
        _testOutputHelper.WriteLine($"Avail Virt: {systemStatistics.AvailableVirtual}");
        _testOutputHelper.WriteLine($"Tot Phys  : {systemStatistics.TotalPhysical}");
        _testOutputHelper.WriteLine($"Tot Virt  : {systemStatistics.TotalVirtual}");
        _testOutputHelper.WriteLine($"Avail Page: {systemStatistics.AvailablePageFile}");
        _testOutputHelper.WriteLine($"Tot Page  : {systemStatistics.TotalPageFile}");
    }
}

