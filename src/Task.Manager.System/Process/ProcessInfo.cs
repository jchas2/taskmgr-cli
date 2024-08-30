using Microsoft.Win32.SafeHandles;

namespace Task.Manager.System.Process;

public struct ProcessInfo
{
    public nint Pid;
    public SafeProcessHandle Handle;
    public int ThreadCount;
    public long BasePriority;
    public nint ParentPid;
    public string ExeName;
    public string FileDescription;
    public string UserName;
    public string CmdLine;
    public long UsedMemory;
    public long DiskUsage;
    // public long DiskOperations;
    // public TimeSpan ProcessorTime;
    // public TimeSpan ProcessorUserTime;
    // public TimeSpan ProcessorKernelTime;
    public ProcessTimeInfo CurrentTimes;
    public ProcessTimeInfo NextTimes;
}
