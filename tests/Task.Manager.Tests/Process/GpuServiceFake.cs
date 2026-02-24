using Task.Manager.System;
using Task.Manager.System.Process;

namespace Task.Manager.Tests.Process;

public sealed class GpuServiceFake : IGpuService
{
    public bool GetGpuMemory(ref SystemStatistics systemStatistics)
    {
        return true;
    }

    public Dictionary<int, long> GetProcessStats()
    {
        return new Dictionary<int, long>();
    }
}