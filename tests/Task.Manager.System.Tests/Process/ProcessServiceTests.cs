using Task.Manager.System.Process;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Tests.Process;

public sealed class ProcessServiceTests
{
    [Fact]
    public void Should_Return_Processes()
    {
        ProcessInfo[] processInfos = new ProcessService().GetProcesses();
        Assert.InRange(processInfos.Length, 1, int.MaxValue);
    }
    
    [Fact]
    public void Should_Get_Process_Properties_For_Current_Process() 
    {
        using SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        ProcessInfo? processInfo = new(currentProcess);

        Assert.NotNull(processInfo);
        ProcessInfoHelpers.AssertProcessInfoProperties(processInfo);
        ProcessInfoHelpers.AssertProcessInfoProperties(currentProcess, processInfo);
    }

    [Fact]
    public void Should_Return_ProcessInfo_By_Id()
    {
        using SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        ProcessInfo? processInfo = new ProcessService().GetProcessById(currentProcess.Id);
        
        Assert.NotNull(processInfo);
        ProcessInfoHelpers.AssertProcessInfoProperties(processInfo);
        ProcessInfoHelpers.AssertProcessInfoProperties(currentProcess, processInfo);
    }

    [Fact]
    public void Should_Have_MainModule_For_Current_Process()
    {
        using SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        ProcessInfo? processInfo = new ProcessService().GetProcessById(currentProcess.Id);
        
        Assert.NotNull(processInfo);
        Assert.NotNull(processInfo.MainModule);
        Assert.NotNull(processInfo.MainModule.FileName);
        Assert.Equal(processInfo.MainModule.ModuleName, processInfo.ModuleName);
    }
}