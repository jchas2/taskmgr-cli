using Microsoft.Win32.SafeHandles;

namespace Task.Manager.System.Process;

public class ProcessInfo
{
    public int Pid { get; set; }
    public SafeProcessHandle? Handle { get; set; }
    public int ThreadCount { get; set; }
    public long BasePriority { get; set; }
    public int ParentPid { get; set; }
    public string? ExeName { get; set; }
    public string? FileDescription { get; set; }
    public string? UserName { get; set; }
    public string? CmdLine { get; set; }
    public long UsedMemory { get; set; }
    public long DiskUsage { get; set; }
    public long DiskOperations { get; set; }
    public double ProcessorTime { get; set; }
    public double ProcessorUserTime { get; set; }
    public double ProcessorKernelTime { get; set; }
    public ProcessTimeInfo PreviousTimes { get; set; }
    public ProcessTimeInfo CurrentTimes { get; set; }
}

