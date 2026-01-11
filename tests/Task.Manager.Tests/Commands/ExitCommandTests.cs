using Task.Manager.Commands;
using Task.Manager.Gui;

namespace Task.Manager.Tests.Commands;

public sealed class ExitCommandTests
{
    [Fact]
    public void Exit_Command_Should_Be_Enabled()
    {
        MainScreen mainScreen = CommandHelper.SetupMainScreen();
        AboutCommand cmd = new("Exit", mainScreen);

        Assert.True(cmd.IsEnabled);
    }
}
