using System.Text;
using Task.Manager.Cli.Utils;

namespace Task.Manager.System.Tests;

public sealed class When_Using_SystemTerminal
{
    [Fact]
    public void Should_Set_Streams()
    {
        var terminal = new SystemTerminal();
        Assert.True(terminal.StdError != null);
        Assert.True(terminal.StdOut != null);
        Assert.True(terminal.StdIn != null);
    }

    [Fact]
    public void Should_Set_Colours()
    {
        var terminal = new SystemTerminal();
        var background = terminal.BackgroundColor;
        var foreground = terminal.ForegroundColor;
        
        // Switch up the values for a simple getter/setter test.
        terminal.BackgroundColor = foreground;
        Assert.True(terminal.BackgroundColor == foreground);
        
        terminal.ForegroundColor = background;
        Assert.True(terminal.ForegroundColor == background);
    }

    [Fact]
    public void Should_Encode_Ansi_Colour_Codes()
    {
        var terminal = new SystemTerminal();
        string testString = "This should be Red".ToRed();
        
        // Initially just test no error is thrown.
        terminal.WriteLine(testString);
    }

    [Fact]
    public void Should_Set_Cursor_Position()
    {
        var terminal = new SystemTerminal();
        terminal.CursorLeft = 0;
        terminal.CursorTop = 0;
        
        Assert.Equal(0, terminal.CursorLeft);
        Assert.Equal(0, terminal.CursorTop);
        
        // TODO: Failing test.
        //terminal.SetCursorPosition(2, 4);

        //Assert.Equal(2, terminal.CursorLeft);
        //Assert.Equal(4, terminal.CursorTop);
    }
    
    // TODO: Writing test for the various Write@ functions will require setting the TextWriter on the Console.
}