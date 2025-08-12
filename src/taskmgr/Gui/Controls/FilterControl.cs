using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;

namespace Task.Manager.Gui.Controls;

public class FilterControl : Control
{
    private readonly Theme theme;
    private const int CommandLength = 6;
    
    public FilterControl(ISystemTerminal terminal, Theme theme) : base(terminal) => this.theme = theme;

    public int NeededWidth { get; private set; }
    
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
        
        nchars += KeyBindControl.Draw(
            "Enter",
            "Done",
            nchars,
            Y,
            CommandLength,
            theme,
            Terminal);

        nchars += KeyBindControl.Draw(
            "Esc",
            "Clear",
            nchars,
            Y,
            CommandLength,
            theme,
            Terminal);

        Terminal.WriteEmptyLineTo(Width - nchars);
        Terminal.SetCursorPosition(left: nchars, top: Y);

        string spacer = "  ";
        string filterCommand = "Filter: ";

        Terminal.BackgroundColor = theme.Background;
        Terminal.ForegroundColor = theme.Foreground;
        Terminal.Write(spacer);
        nchars += spacer.Length;
        
        Terminal.BackgroundColor = theme.BackgroundHighlight;
        Terminal.ForegroundColor = theme.ForegroundHighlight;
        Terminal.Write(filterCommand);
        nchars += filterCommand.Length;
        
        NeededWidth = nchars;
    }
}
