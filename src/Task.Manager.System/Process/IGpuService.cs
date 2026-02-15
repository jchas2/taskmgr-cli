namespace Task.Manager.System.Process;

public interface IGpuService
{
    Dictionary<int, long> GetStats();
}
