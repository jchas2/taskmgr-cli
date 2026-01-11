using Task.Manager.Commands;
using Task.Manager.Gui;
using Task.Manager.Gui.Controls;
using Task.Manager.System.Screens;

namespace Task.Manager.Tests.Commands;

public sealed class HelpCommandTests
{
    [Fact]
    public void Help_Command_Should_Be_Enabled()
    {
        (ScreenApplication screenApp, MainScreen mainScreen) = CommandHelper.SetupMainScreenWithScreenApp();
        HelpCommand cmd = new("Help", screenApp);
        
        Assert.True(cmd.IsEnabled);
    }
}
