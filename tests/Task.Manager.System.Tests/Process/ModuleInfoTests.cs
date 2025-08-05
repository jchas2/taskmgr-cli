using Task.Manager.System.Process;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Tests.Process;

public sealed class ModuleInfoTests
{
    [Fact]
    public void Should_Return_Modules_For_Current_Process()
    {
        using var currentProcess = SysDiag::Process.GetCurrentProcess();
        var modules = ModuleInfo.GetModules(currentProcess.Id);
        Assert.True(modules.Count > 0);
    }
}