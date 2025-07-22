using Task.Manager.Interop.Win32;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public static partial class ProcessUtils
{
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