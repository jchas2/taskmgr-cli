using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public class ProcessInfo(SysDiag::Process process)
{
    //private readonly SysDiag::Process process;
    
    //public ProcessInfo(SysDiag::Process process) => this.process = process;
    
    public int Pid => process.Id;
    public nint Handle => process.Handle;
    public int ParentPid = 0;
    public string ProcessName => process.ProcessName;
    public string ModuleName => process.MainModule!.ModuleName;
    public string FileDescription => ProcessUtils.GetProcessProductName(process);
    public string UserName => ProcessUtils.GetProcessUserName(process);
    public string CmdLine => ProcessUtils.GetProcessCommandLine(process);
    public DateTime StartTime => process.StartTime;
    public int ThreadCount => process.Threads.Count;
    public uint HandleCount => ProcessUtils.GetHandleCount(process);
    public long BasePriority => process.BasePriority;
    public long UsedMemory => process.WorkingSet64;
#if __WIN32__             
    public long KernelTime => process.PrivilegedProcessorTime.Ticks;
    public long UserTime => process.UserProcessorTime.Ticks;
#endif
#if __APPLE__
    // NOTE: The calls to host_statistics64() for System CPU don't align
    // with the framework calls below. To align the tick results * 1000. 
    public long KernelTime => process.PrivilegedProcessorTime.Ticks * 1000;
    public long UserTime => process.UserProcessorTime.Ticks * 1000;
#endif
    public ulong DiskOperations => ProcessUtils.GetProcessIoOperations(process);
}


