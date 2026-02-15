using Task.Manager.System.Process;

namespace Task.Manager.Tests.Process;

public sealed class GpuServiceFake : IGpuService
{
    public Dictionary<int, long> GetStats()
    {
        return new Dictionary<int, long>();
    }
}