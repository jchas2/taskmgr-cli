namespace Task.Manager.System.Process;

public interface IModuleService
{
    List<ModuleInfo> GetModules(int pid);
}