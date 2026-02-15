using Task.Manager.Interop.Mach;

namespace Task.Manager.System.Process;

public partial class GpuService : IGpuService
{
    public Dictionary<int, long> GetStats() => GetStatsInternal();
}
