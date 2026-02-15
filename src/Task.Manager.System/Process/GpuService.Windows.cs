namespace Task.Manager.System.Process;

public partial class GpuService
{
#if __WIN32__
    internal Dictionary<int, long> GetStatsInternal()
    {
        Dictionary<int, long> gpuInfo = new();
        return gpuInfo;
    }
#endif
}