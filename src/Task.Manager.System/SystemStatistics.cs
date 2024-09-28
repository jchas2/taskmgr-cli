namespace Task.Manager.System;

public class SystemStatistics
{
    /* Memory */
    public ulong AvailablePhysical { get; set; }
    public ulong AvailablePageFile { get; set; }
    public ulong AvailableVirtual { get; set; }
    public ulong TotalPhysical { get; set; }
    public ulong TotalPageFile { get; set; }
    public ulong TotalVirtual { get; set; }
    
    /* Cpu */
    public double CpuFrequency { get; set; }
    private ulong CpuCores { get; set; }
    public string CpuName { get; set; } = string.Empty;
    
    /* System */
    public string MachineName { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    
    /* Network */
    public string PublicIPv4Address { get; set; } = string.Empty;
    public string PrivateIPv4Address { get; set; } = string.Empty;
}
