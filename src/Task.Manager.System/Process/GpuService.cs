using Task.Manager.Interop.Mach;

namespace Task.Manager.System.Process;

public partial class GpuService : IGpuService
{
    public Dictionary<int, long> GetProcessStats() => GetProcessStatsInternal();
    public bool GetGpuMemory(ref SystemStatistics systemStatistics) => GetGpuMemoryInternal(ref systemStatistics);
}
