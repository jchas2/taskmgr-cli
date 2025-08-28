﻿using Moq;
using Task.Manager.System.Controls.InputBox;
using InputBoxControl = Task.Manager.System.Controls.InputBox.InputBox;

namespace Task.Manager.System.Tests.Controls.InputBox;

public sealed class InputBoxTests
{
    [Fact]
    public void Should_Construct_Default()
    {
        Mock<ISystemTerminal> terminal = TerminalMock.Setup();
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
        Mock<ISystemTerminal> terminal = TerminalMock.Setup();
        
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
            { ControlHelper.GetConsoleKeyInfo(ConsoleKey.Enter),      InputBoxResult.Enter },
            { ControlHelper.GetConsoleKeyInfo(ConsoleKey.Escape),     InputBoxResult.Cancel },
            { ControlHelper.GetConsoleKeyInfo(ConsoleKey.Backspace),  InputBoxResult.None },
            { ControlHelper.GetConsoleKeyInfo(ConsoleKey.Delete),     InputBoxResult.None },
            { ControlHelper.GetConsoleKeyInfo(ConsoleKey.LeftArrow),  InputBoxResult.None },
            { ControlHelper.GetConsoleKeyInfo(ConsoleKey.RightArrow), InputBoxResult.None },
            { ControlHelper.GetConsoleKeyInfo(ConsoleKey.Insert),     InputBoxResult.None }
        };
    
    [Theory]
    [MemberData(nameof(KeyPressData))]
    public void KeyPress_Should_Set_InputBoxResult(ConsoleKeyInfo keyInfo, InputBoxResult expectedResult)
    {
        Mock<ISystemTerminal> terminal = TerminalMock.Setup();
        
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
        Mock<ISystemTerminal> terminal = TerminalMock.Setup();

        InputBoxControl control = new(terminal.Object) {
            X = 0, 
            Y = 0, 
            Width = 42, 
            Height = 1,
            Title = "Enter Text"
        };
        
        control.ShowInputBox();
        
        bool handled = false;

        control.KeyPressed(
            new ConsoleKeyInfo(
                (char)ConsoleKey.H, 
                ConsoleKey.H, 
                shift: false, 
                alt: false, 
                control: false), 
            ref handled);
         
        terminal.Verify(t => t.SetCursorPosition(0, 0), Times.AtLeastOnce);
        terminal.Verify(t => t.Write("H"), Times.Once);
        
        Assert.Equal("H", control.Text);
        Assert.True(handled);
    }
}
