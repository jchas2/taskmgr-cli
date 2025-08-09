using System.Text;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;

namespace Task.Manager.Gui.Controls;

public sealed class CommandControl : Control
{
    private readonly Theme _theme;
    private const string CommandText = "          Help      Setup     Sort      Filter    Info      End Task  ";
    private const int CommandLength = 10;

    public CommandControl(ISystemTerminal terminal, Theme theme) : base(terminal) => _theme = theme;

    protected override void OnDraw()
    {
        try {
            Control.DrawingLockAcquire(); 
            OnDrawInternal();
        }
        finally {
            Control.DrawingLockRelease();
        }
    }
    
    private void OnDrawInternal()
    {
        using TerminalColourRestorer _ = new();
        
        Terminal.SetCursorPosition(left: X, top: Y);
        int nchars = 0;
        
        for (int i = 1; i <= (CommandText.Length / CommandLength) - 1; i++) {

            nchars += KeyBindControl.Draw(
                $"F{i}",
                CommandText.Substring(i * CommandLength, CommandLength),
                nchars,
                Y,
                CommandLength,
                _theme,
                Terminal);
        }
        
        Terminal.WriteEmptyLineTo(Width - nchars);
    }
}