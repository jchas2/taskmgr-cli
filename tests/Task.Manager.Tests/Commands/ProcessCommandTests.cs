using System.Runtime.CompilerServices;
using Task.Manager.Commands;
using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Tests.Commands;

public sealed class ProcessCommandTests
{
    [Fact]
    public void Process_Command_Should_Be_Disabled_By_Default_When_ProcessControl_Empty()
    {
        MainScreen mainScreen = CommandHelper.SetupMainScreen();
        ProcessCommand cmd = new("Process", mainScreen);
        
        Assert.IsType<ProcessControl>(mainScreen.GetActiveControl);
        Assert.False(cmd.IsEnabled);
    }
    
    [Fact]
    public void Process_Command_Should_Return_Negative_ProcessId_When_ProcessControl_Empty()
    {
        MainScreen mainScreen = CommandHelper.SetupMainScreen();
        ProcessCommand cmd = new("Process", mainScreen);
        
        Assert.IsType<ProcessControl>(mainScreen.GetActiveControl);
        Assert.Equal(-1, cmd.SelectedProcessId);
    }
}
