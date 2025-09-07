﻿using Moq;
using Task.Manager.System.Controls;

namespace Task.Manager.System.Tests.Controls;

public class ControlTests
{
    private static readonly Mock<ISystemTerminal> SystemTerminalSingleton = new();
    
    [Fact]
    public void Should_Construct_Default()
    {
        Mock<ISystemTerminal> terminalMock = TerminalMock.Setup();
        Control control = new(terminalMock.Object);

        Assert.Equal(ConsoleColor.Black, control.BackgroundColour);
        Assert.Equal(0, control.ControlCount);
        Assert.Empty(control.Controls);
        Assert.Equal(ConsoleColor.White, control.ForegroundColour);
        Assert.Equal(0, control.Height);
        Assert.True(control.Visible);
        Assert.Equal(0, control.Width);
        Assert.Equal(0, control.X);
        Assert.Equal(0, control.Y);
    }
    
    public static List<Control> GetControlData()
        => new() {
            new Control(SystemTerminalSingleton.Object),
            new Control(SystemTerminalSingleton.Object),
            new Control(SystemTerminalSingleton.Object)
        };

    [Fact]
    public void Should_Add_All_Items()
    {
        var control = new Control(SystemTerminalSingleton.Object);
        control.Controls.AddRange(GetControlData().ToArray());
        
        Assert.True(control.Controls.Count == 3);
    }
}
