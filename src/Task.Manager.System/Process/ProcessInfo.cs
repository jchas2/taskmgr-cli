using Microsoft.Win32.SafeHandles;

namespace Task.Manager.System.Process;

public struct ProcessInfo
{
    public ProcessInfo() { }

    public int Pid { get; set; }
    //public IntPtr? Handle { get; set; }
    public int ThreadCount { get; set; }
    public uint HandleCount { get; set; }
    public long BasePriority { get; set; }
    public int ParentPid { get; set; }
    public DateTime StartTime { get; set; }

    public string ExeName { get; set; } = string.Empty;
    public string FileDescription { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string CmdLine { get; set; } = string.Empty;
    
    public long DiskUsage { get; set; }
    public ulong DiskOperations { get; set; }

    public long UsedMemory { get; set; }

    public double CpuTimePercent { get; set; }
    public double CpuUserTimePercent { get; set; }
    public double CpuKernelTimePercent { get; set; }
    
    public long PrevCpuKernelTime { get; set; }
    public long PrevCpuUserTime { get; set; }
    
    public long CurrCpuKernelTime { get; set; }
    public long CurrCpuUserTime { get; set; }
}


