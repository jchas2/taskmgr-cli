using Task.Manager.Commands;
using Task.Manager.Gui;

namespace Task.Manager.Tests.Commands;

public sealed class AboutCommandTests
{
    [Fact]
    public void About_Command_Should_Be_Enabled()
    {
        MainScreen mainScreen = CommandHelper.SetupMainScreen();
        AboutCommand cmd = new("About", mainScreen);
        
        Assert.True(cmd.IsEnabled);
    }
}
