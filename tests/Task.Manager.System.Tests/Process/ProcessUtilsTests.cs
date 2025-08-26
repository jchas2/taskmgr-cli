using System.Runtime.Versioning;
using Task.Manager.System.Process;
using Task.Manager.Tests.Common;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Tests.Process;

public partial class ProcessUtilsTests
{
    [SkippableFact]
    [SupportedOSPlatform("windows")]
    public void Should_End_Windows_Task()
    {
        using FileCleanupHelper cleanupHelper = new();
        string fileName = cleanupHelper.GetTempFile(".bat");
        Should_End_Task(fileName);
    }

    [SkippableFact]
    [SupportedOSPlatform("macos")]
    public void Should_End_MacOS_Task()
    {
        // TODO: Debug on MacOS.
        // using FileCleanupHelper cleanupHelper = new();
        // string fileName = cleanupHelper.GetTempFile(".sh");
        // Should_End_Task(fileName);
    }

    private void Should_End_Task(string fileName)
    {
        CreateScriptFile(fileName);
        
        using SysDiag::Process? process = SysDiag::Process.Start(
            new SysDiag::ProcessStartInfo {
                UseShellExecute = true,
                FileName = fileName
            });
        
        Assert.NotNull(process);
        Thread.Sleep(millisecondsTimeout: 200);

        bool terminated = ProcessUtils.EndTask(process.Id, timeOutMilliseconds: 500);
        Assert.True(terminated);
    } 
    
    [SkippableFact]
    [SupportedOSPlatform("windows")]
    public void Should_Get_Handle_Count()
    {
        uint handleCount = ProcessUtils.GetHandleCount(SysDiag::Process.GetCurrentProcess());
        Assert.InRange(handleCount, (uint)1, uint.MaxValue);
    }

    [Fact]
    public void Should_Get_Process_By_Pid()
    {
        SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        bool result = ProcessUtils.TryGetProcessByPid(currentProcess.Id, out SysDiag::Process? process);
        
        Assert.True(result);
        Assert.NotNull(process);
    }

    [Fact]
    public void Should_Get_Process_Command_Line()
    {
        SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        string commandLine = ProcessUtils.GetProcessCommandLine(currentProcess.MainModule!, string.Empty);
        
        Assert.NotEmpty(commandLine);
    }

    [Fact]
    public void Should_Get_Process_IO_Operations()
    {
        SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        ulong ops = ProcessUtils.GetProcessIoOperations(currentProcess);
        
        Assert.InRange(ops, (ulong)0, uint.MaxValue);
    }

    [Fact]
    public void Should_Get_Process_Product_Name()
    {
        SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        
        string productName = ProcessUtils.GetProcessProductName(
            currentProcess.MainModule!,
            currentProcess.Id,
            string.Empty);

         Assert.NotEmpty(productName);
    }

    [Fact]
    public void Should_Get_Process_User_Name()
    {
        SysDiag::Process currentProcess = SysDiag::Process.GetCurrentProcess();
        string userName = ProcessUtils.GetProcessUserName(currentProcess);
        
        Assert.NotEmpty(userName);
    }
}
