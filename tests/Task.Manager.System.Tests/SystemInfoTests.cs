using System.Globalization;
using Xunit.Abstractions;

namespace Task.Manager.System.Tests;

public sealed class SystemInfoTests
{
    private readonly ITestOutputHelper testOutputHelper;
    
    public SystemInfoTests(ITestOutputHelper testOutputHelper) => this.testOutputHelper = testOutputHelper;
    
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

        testOutputHelper.WriteLine($"CPU    : {systemStatistics.CpuName}");
        testOutputHelper.WriteLine($"Mhz    : {systemStatistics.CpuFrequency.ToString(CultureInfo.CurrentCulture)}");
        testOutputHelper.WriteLine($"Cores  : {systemStatistics.CpuCores}");
        testOutputHelper.WriteLine($"OS     : {systemStatistics.OsVersion}");
        testOutputHelper.WriteLine($"Machine: {systemStatistics.MachineName}");

        testOutputHelper.WriteLine($"Priv IP: {systemStatistics.PrivateIPv4Address}");
        testOutputHelper.WriteLine($"Pub IP : {systemStatistics.PublicIPv4Address}");
        
        result = systemInfo.GetSystemMemory(ref systemStatistics);
        Assert.True(result);
        
        testOutputHelper.WriteLine($"Avail Phys: {systemStatistics.AvailablePhysical}");
        testOutputHelper.WriteLine($"Avail Virt: {systemStatistics.AvailableVirtual}");
        testOutputHelper.WriteLine($"Tot Phys  : {systemStatistics.TotalPhysical}");
        testOutputHelper.WriteLine($"Tot Virt  : {systemStatistics.TotalVirtual}");
        testOutputHelper.WriteLine($"Avail Page: {systemStatistics.AvailablePageFile}");
        testOutputHelper.WriteLine($"Tot Page  : {systemStatistics.TotalPageFile}");
    }
}

