using Task.Manager.Commands;
using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Tests.Commands;

public sealed class ProcessSortCommandTests
{
    [Fact]
    public void ProcessSort_Command_Should_Be_Enabled_When_ProcessControl_IsActive_But_Empty()
    {
        MainScreen mainScreen = CommandHelper.SetupMainScreen();
        ProcessSortCommand cmd = new("Sort", mainScreen);
        
        Assert.IsType<ProcessControl>(mainScreen.GetActiveControl);
        Assert.True(cmd.IsEnabled);
    }
}