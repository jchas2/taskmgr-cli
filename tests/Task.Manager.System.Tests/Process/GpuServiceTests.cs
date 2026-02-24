using Task.Manager.System.Process;

namespace Task.Manager.System.Tests.Process;

public sealed class GpuServiceTests
{
    [Fact]
    public void Should_Run_Gpu_Process_Stats()
    {
        GpuService service = new();
        Dictionary<int, long> data = service.GetProcessStats();
        Assert.NotNull(data);
    }

    [Fact]
    public void Should_Run_Gpu_Memory_Stats()
    {
        SystemStatistics systemStatistics = new();
        GpuService service = new();
        service.GetGpuMemory(ref systemStatistics);
    }
}
