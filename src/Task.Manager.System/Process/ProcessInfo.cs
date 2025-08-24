using Task.Manager.Cli.Utils;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public class ProcessInfo(SysDiag::Process process)
{
    private SysDiag::ProcessModule? mainModule;
    private bool mainModuleInspected = false;
    private string? fileDescription;
    private string? moduleName;
    private string? userName;
    private string? cmdLine;

    internal SysDiag::ProcessModule? MainModule
    {
        get {
            // This is a hotspot in the code hence the value is only
            // inspected once and cached.
            if (!mainModuleInspected) {
                try {
                    mainModule = process.MainModule;
                }
                catch (Exception ex) {
                    ExceptionHelper.HandleException(ex);
                    mainModule = null;
                }
                mainModuleInspected = true;
            }
            return mainModule;
        }
    } 
    
    public int Pid => process.Id;
    public nint Handle => process.Handle;
    public int ParentPid => 0;
    public string ProcessName => process.ProcessName;

    public string ModuleName
    {
        get {
            if (string.IsNullOrEmpty(moduleName)) {
                if (MainModule != null) {
                    moduleName = MainModule.ModuleName;
                }
                else {
                    moduleName = ProcessName;
                }
            }
            return moduleName;
        }
    } 

    public string FileDescription 
    {
        get {
            if (string.IsNullOrEmpty(fileDescription)) {
                if (MainModule != null) {
                    fileDescription = ProcessUtils.GetProcessProductName(
                        MainModule,
                        Pid,
                        ProcessName);
                }
                else {
                    fileDescription = ProcessName;
                }
            }
            return fileDescription;
        }
    }
    
    public string UserName
    {
        get {
            if (string.IsNullOrEmpty(userName)) {
                userName = ProcessUtils.GetProcessUserName(process);
            }
            return userName;
        }
    }

    public string CmdLine
    {
        get {
            if (string.IsNullOrEmpty(cmdLine)) {
                if (MainModule != null) {
                    cmdLine = ProcessUtils.GetProcessCommandLine(MainModule, ProcessName);    
                }
                else {
                    cmdLine = ProcessName;
                }
            }
            return cmdLine;
        }
    }
    
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

