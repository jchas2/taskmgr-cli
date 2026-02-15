namespace Task.Manager.System;

public struct SystemStatistics()
{
    // Memory.
    public ulong AvailablePhysical { get; set; } = 0;
    public ulong AvailablePageFile { get; set; } = 0;
    public ulong AvailableVirtual { get; set; } = 0;
    public ulong TotalPhysical { get; set; } = 0;
    public ulong TotalPageFile { get; set; } = 0;
    public ulong TotalVirtual { get; set; } = 0;

    // Cpu Info.
    public double CpuFrequency { get; set; } = 0;
    public ulong CpuCores { get; set; } = 0;
    public string CpuName { get; set; } = string.Empty;
    
    // Cpu % Times.
    public double CpuPercentIdleTime { get; set; } = 0;
    public double CpuPercentKernelTime { get; set; } = 0;
    public double CpuPercentUserTime { get; set; } = 0;
    
    // Gpu.
    public long TotalGpuMemory { get; set; } = 0;
    public long AvailableGpuMemory { get; set; } = 0;
    public double GpuPercentTime { get; set; } = 0;

    // System.
    public string MachineName { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    
    // Network.
    public string PublicIPv4Address { get; set; } = string.Empty;
    public string PrivateIPv4Address { get; set; } = string.Empty;
    
    // Disk.
    public long DiskUsage { get; set; } = 0;
    
    // Processes.
    public int ProcessCount { get; set; } = 0;
    public int ThreadCount { get; set; } = 0;
}
