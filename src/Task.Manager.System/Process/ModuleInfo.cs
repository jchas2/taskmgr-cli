using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public sealed partial class ModuleInfo
{
    public string FileName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;


    public static List<ModuleInfo> GetModules(int pid)
    {
        if (false == TryGetProcessByPid(pid, out SysDiag::Process? process) ||
            process == null) {
            return [];
        }
        
        if (false == GetProcessModulesInternal(process, out var moduleInfos)) {
            return [];
        }
        
        return moduleInfos;
    }

    private static bool TryGetProcessByPid(int pid, out SysDiag::Process? process)
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

    private static bool TryGetProcessModules(SysDiag::Process process, out SysDiag::ProcessModule[] modules)
    {
        try {
            modules = process.Modules.Cast<SysDiag::ProcessModule>().ToArray();
            return true;
        }
#pragma warning disable CS0168 // The variable is declared but never used
        catch (Exception e) {
            SysDiag::Debug.WriteLine($"Failed Modules() for Pid {process.Id} with {e}");
            modules = [];
            return false;
        }
#pragma warning restore CS0168 // The variable is declared but never used
    }
}