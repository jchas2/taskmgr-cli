using Task.Manager.Cli.Utils;
using Task.Manager.Interop.Win32;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public static partial class ProcessUtils
{
    public static bool EndTask(int pid, int timeOutMilliseconds)
    {
        if (!TryGetProcessByPid(pid, out SysDiag::Process? process) || process == null) {
            return false;
        }
        
        process.Kill(entireProcessTree: true);
        return process.WaitForExit(timeOutMilliseconds);
    }
    
    internal static uint GetHandleCount(SysDiag::Process process) => GetHandleCountInternal(process);
    
    internal static bool TryGetProcessByPid(int pid, out SysDiag::Process? process)
    {
        try {
            process = SysDiag::Process.GetProcessById(pid);
            return true;
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex, $"Failed GetProcessById() for Pid {pid}");
            process = null;
            return false;
        }
    }
}
