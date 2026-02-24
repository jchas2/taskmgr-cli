namespace Task.Manager.System.Process;

public interface IGpuService
{
    bool GetGpuMemory(ref SystemStatistics systemStatistics);
    Dictionary<int, long> GetProcessStats();
}
