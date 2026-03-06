using Task.Manager.Interop.Mach;

namespace Task.Manager.System.Process;

public static partial class GpuService
{
    public static Dictionary<int, long> GetProcessStats() => GetProcessStatsInternal();
}
