using Task.Manager.System.Process;

namespace Task.Manager.System.Tests.Process;

public sealed class GpuServiceTests
{
    [Fact]
    public void Should_Run_Gpu_Stats()
    {
        GpuService service = new();
        var data = service.GetStats();

        Assert.NotNull(data);
    }
}