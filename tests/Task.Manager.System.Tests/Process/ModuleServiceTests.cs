using System.Runtime.Versioning;
using Task.Manager.System.Process;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Tests.Process;

public sealed class ModuleServiceTests
{
    // Note: Tests only run on Windows as MacOS only includes the Main Module.
    [SkippableFact]
    [SupportedOSPlatform("windows")]
    public void Should_Return_Modules_For_Current_Process() 
    {
        using SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        List<ModuleInfo> modules = new ModuleService().GetModules(currentProcess.Id);
        Assert.InRange(modules.Count, 1, int.MaxValue);
    }
    
    [SkippableFact]
    [SupportedOSPlatform("windows")]
    public void Should_Enumerate_Module_Properties_For_Current_Process() 
    {
        using SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        List<ModuleInfo> modules = new ModuleService().GetModules(currentProcess.Id);

        foreach (ModuleInfo moduleInfo in modules) {
            Assert.NotNull(moduleInfo.ModuleName);
            Assert.NotEmpty(moduleInfo.ModuleName);
            
            Assert.NotNull(moduleInfo.FileName);
            Assert.NotEmpty(moduleInfo.FileName);
        }
    }
}