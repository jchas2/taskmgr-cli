using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public partial class ModuleService
{
    public virtual List<ModuleInfo> GetModules(int pid)
    {
        if (!ProcessUtils.TryGetProcessByPid(pid, out SysDiag::Process? process) ||
            process == null) {
            return [];
        }
        
        if (!GetModulesInternal(process, out var moduleInfos)) {
            return [];
        }
        
        return moduleInfos;
    }
}