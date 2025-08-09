using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;

namespace Task.Manager.Gui.Controls;

public static class KeyBindControl
{
    public static int Draw(
        string keyBinding,
        string text,
        int x,
        int y,
        int width,
        Theme theme,
        ISystemTerminal terminal)
    {
        terminal.BackgroundColor = theme.Background;
        terminal.ForegroundColor = theme.Foreground;
        terminal.Write(keyBinding);
        int nchars = keyBinding.Length;
        
        terminal.BackgroundColor = theme.BackgroundHighlight;
        terminal.ForegroundColor = theme.ForegroundHighlight;
        terminal.Write(text.CentreWithLength(width));
        nchars += width;
        
        return nchars;
    }
}
