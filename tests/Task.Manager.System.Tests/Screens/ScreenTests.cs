using Moq;
using Task.Manager.System.Screens;
using Task.Manager.System.Tests.Controls;

namespace Task.Manager.System.Tests.Screens;

public sealed class ScreenTests
{
    public class TestScreen1(ISystemTerminal systemTerminal) : Screen(systemTerminal);
    public class TestScreen2(ISystemTerminal systemTerminal) : Screen(systemTerminal);

    [Fact]
    public void Should_Construct_Default()
    {
        Mock<ISystemTerminal> terminalMock = TerminalMock.Setup();
        TestScreen1 testScreen = new(terminalMock.Object);

        Assert.Equal(ConsoleColor.Black, testScreen.BackgroundColour);
        Assert.Equal(0, testScreen.ControlCount);
        Assert.Empty(testScreen.Controls);
        Assert.True(testScreen.CursorVisible);
        Assert.Equal(ConsoleColor.White, testScreen.ForegroundColour);
        Assert.Equal(0, testScreen.Height);
        Assert.True(testScreen.Visible);
        Assert.Equal(0, testScreen.Width);
        Assert.Equal(0, testScreen.X);
        Assert.Equal(0, testScreen.Y);
    }
}
