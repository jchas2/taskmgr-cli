using Moq;
using Task.Manager.System.Controls.InputBox;
using InputBoxControl = Task.Manager.System.Controls.InputBox.InputBox;

namespace Task.Manager.System.Tests.Controls.InputBox;

public sealed class InputBoxTests
{
    private Mock<ISystemTerminal> SetupTerminalMock()
    {
        Mock<ISystemTerminal> terminal = new();
        terminal.Setup(t => t.WindowHeight).Returns(24);
        terminal.Setup(t => t.WindowWidth).Returns(80);
        
        return terminal;
    }
    
    [Fact]
    public void Should_Construct_Default()
    {
        Mock<ISystemTerminal> terminal = SetupTerminalMock();
        InputBoxControl control = new(terminal.Object);
        
        Assert.Equal(ConsoleColor.Black, control.BackgroundColour);
        Assert.Equal(0, control.ControlCount);
        Assert.Empty(control.Controls);
        Assert.Equal(ConsoleColor.White, control.ForegroundColour);
        Assert.Equal(0, control.Height);
        Assert.Equal(InputBoxResult.Enter, control.Result);
        Assert.NotNull(control.Text);
        Assert.Empty(control.Text);
        Assert.NotNull(control.Title);
        Assert.Empty(control.Title);
        Assert.True(control.Visible);
        Assert.Equal(0, control.Width);
        Assert.Equal(0, control.X);
        Assert.Equal(0, control.Y);
    }

    [Fact]
    public void Should_Set_Initial_Properties()
    {
        Mock<ISystemTerminal> terminal = SetupTerminalMock();
        
        InputBoxControl control = new(terminal.Object) {
            BackgroundColour = ConsoleColor.Gray,
            ForegroundColour = ConsoleColor.Black,
            Height = 1,
            Title = "Enter Text",
            Visible =  true,
            Width = 42,
            X = 5,
            Y = 10
        };

        Assert.Equal(ConsoleColor.Gray, control.BackgroundColour);
        Assert.Equal(ConsoleColor.Black, control.ForegroundColour);
        Assert.Equal(1, control.Height);
        Assert.Equal("Enter Text", control.Title);
        Assert.True(control.Visible);
        Assert.Equal(42, control.Width);
        Assert.Equal(5, control.X);
        Assert.Equal(10, control.Y);
    }

    public static TheoryData<ConsoleKeyInfo, InputBoxResult> KeyPressData()
        => new()
        {
            { new ConsoleKeyInfo((char)ConsoleKey.Enter,      ConsoleKey.Enter,      shift: false, alt: false, control: false), InputBoxResult.Enter },
            { new ConsoleKeyInfo((char)ConsoleKey.Escape,     ConsoleKey.Escape,     shift: false, alt: false, control: false), InputBoxResult.Cancel },
            { new ConsoleKeyInfo((char)ConsoleKey.Backspace,  ConsoleKey.Backspace,  shift: false, alt: false, control: false), InputBoxResult.None },
            { new ConsoleKeyInfo((char)ConsoleKey.Delete,     ConsoleKey.Delete,     shift: false, alt: false, control: false), InputBoxResult.None },
            { new ConsoleKeyInfo((char)ConsoleKey.LeftArrow,  ConsoleKey.LeftArrow,  shift: false, alt: false, control: false), InputBoxResult.None },
            { new ConsoleKeyInfo((char)ConsoleKey.RightArrow, ConsoleKey.RightArrow, shift: false, alt: false, control: false), InputBoxResult.None },
            { new ConsoleKeyInfo((char)ConsoleKey.Insert,     ConsoleKey.Insert,     shift: false, alt: false, control: false), InputBoxResult.None }
        };
    
    [Theory]
    [MemberData(nameof(KeyPressData))]
    public void KeyPress_Should_Set_InputBoxResult(ConsoleKeyInfo keyInfo, InputBoxResult expectedResult)
    {
        Mock<ISystemTerminal> terminal = SetupTerminalMock();
        
        InputBoxControl control = new(terminal.Object) {
            X = 0, 
            Y = 0, 
            Width = 42, 
            Height = 1,
            Title = "Enter Text"
        };

        bool handled = false;
        control.KeyPressed(keyInfo, ref handled);

        Assert.Equal(expectedResult, control.Result);
        Assert.True(handled);
    }

    [Fact]
    public void Should_Draw()
    {
        Mock<ISystemTerminal> terminal = SetupTerminalMock();

        InputBoxControl control = new(terminal.Object) {
            X = 0, 
            Y = 0, 
            Width = 42, 
            Height = 1,
            Title = "Enter Text"
        };
        
        bool handled = false;

        control.KeyPressed(
            new ConsoleKeyInfo(
                (char)ConsoleKey.H, 
                ConsoleKey.H, 
                shift: false, 
                alt: false, 
                control: false), 
            ref handled);

        control.ShowInputBox();
        
        terminal.Verify(t => t.SetCursorPosition(0, 0), Times.AtLeastOnce);
        
        Assert.Equal("H", control.Text);
        Assert.True(handled);
    }
}
