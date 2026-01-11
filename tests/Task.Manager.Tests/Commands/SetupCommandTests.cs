using Task.Manager.Commands;
using Task.Manager.Gui;
using Task.Manager.System.Screens;

namespace Task.Manager.Tests.Commands;

public sealed class SetupCommandTests
{
    [Fact]
    public void Help_Command_Should_Be_Enabled()
    {
        (ScreenApplication screenApp, MainScreen mainScreen) = CommandHelper.SetupMainScreenWithScreenApp();
        SetupCommand cmd = new("Setup", screenApp);
        
        Assert.True(cmd.IsEnabled);
    }
}