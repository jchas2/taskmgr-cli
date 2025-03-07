namespace Task.Manager.System;

public sealed class SystemStatistics
{
    /* Memory */
    public ulong AvailablePhysical { get; set; }
    public ulong AvailablePageFile { get; set; }
    public ulong AvailableVirtual { get; set; }
    public ulong TotalPhysical { get; set; }
    public ulong TotalPageFile { get; set; }
    public ulong TotalVirtual { get; set; }
    
    /* Cpu Info */
    public double CpuFrequency { get; set; }
    public ulong CpuCores { get; set; }
    public string CpuName { get; set; } = string.Empty;
    
    /* Cpu % Times */
    public double CpuPercentIdleTime { get; set; }
    public double CpuPercentKernelTime { get; set; }
    public double CpuPercentUserTime { get; set; }
    
    /* System */
    public string MachineName { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    
    /* Network */
    public string PublicIPv4Address { get; set; } = string.Empty;
    public string PrivateIPv4Address { get; set; } = string.Empty;
    
    /* Processes */
    public int ProcessCount { get; set; }
    public int GhostProcessCount { get; set; }
    public int ThreadCount { get; set; }
}
