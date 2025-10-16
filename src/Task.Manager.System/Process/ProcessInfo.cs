using System.Reflection;
using Task.Manager.Cli.Utils;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public class ProcessInfo(SysDiag::Process process)
{
    private SysDiag::ProcessModule? mainModule;
    private bool mainModuleInspected = false;
    private string? fileDescription;
    private string? fileName;
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
                moduleName = MainModule?.ModuleName ?? ProcessName;
            }
            return moduleName;
        }
    }

    public string FileName
    {
        get {
            if (string.IsNullOrEmpty(fileName)) {
                fileName = MainModule?.FileName ?? ProcessName;
            }
            return fileName;
        }
    }

    public string FileDescription 
    {
        get {
            if (string.IsNullOrEmpty(fileDescription)) {
                fileDescription = MainModule != null 
                    ? ProcessUtils.GetProcessProductName(
                        MainModule,
                        Pid,
                        ProcessName)
                    : ProcessName;
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
                cmdLine = MainModule != null 
                    ? ProcessUtils.GetProcessCommandLine(MainModule, ProcessName)
                    : ProcessName;
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
    public long KernelTime => 
        process.PrivilegedProcessorTime.Ticks / (Constants.ProcessDelayInMilliseconds * Environment.ProcessorCount) * 100;
    public long UserTime => 
        process.UserProcessorTime.Ticks / (Constants.ProcessDelayInMilliseconds * Environment.ProcessorCount) * 100;
#endif
    public ulong DiskOperations => ProcessUtils.GetProcessIoOperations(process);
}

