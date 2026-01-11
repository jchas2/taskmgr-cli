using Moq;
using Task.Manager.Commands;
using Task.Manager.Gui;
using Task.Manager.Gui.Controls;
using Task.Manager.System.Screens;
using Xunit.Abstractions;

namespace Task.Manager.Tests.Commands;

public sealed class FilterCommandTests
{
    [Fact]
    public void Filter_Command_Should_Be_Enabled_When_ProcessControl_IsActive_But_Empty()
    {
        MainScreen mainScreen = CommandHelper.SetupMainScreen();
        FilterCommand cmd = new("Filter", mainScreen);
        
        Assert.IsType<ProcessControl>(mainScreen.GetActiveControl);
        Assert.True(cmd.IsEnabled);
    }
}
