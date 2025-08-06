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
    
    public static uint GetHandleCount(SysDiag::Process process) => GetHandleCountInternal(process);
    
    public static bool TryGetProcessByPid(int pid, out SysDiag::Process? process)
    {
        try {
            process = SysDiag::Process.GetProcessById(pid);
            return true;
        }
#pragma warning disable CS0168 // The variable is declared but never used
        catch (Exception e) {
            SysDiag::Debug.WriteLine($"Failed GetProcessById() for Pid {pid} with {e}");
            process = null;
            return false;
        }
#pragma warning restore CS0168 // The variable is declared but never used
    }
}