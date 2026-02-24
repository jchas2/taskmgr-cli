namespace Task.Manager.System.Process;

public sealed class GpuInfo
{
    public int Pid { get; set; }
    public long TotalGpuTime { get; set; }
    public float TotalGpuPercent { get; set; }
}
