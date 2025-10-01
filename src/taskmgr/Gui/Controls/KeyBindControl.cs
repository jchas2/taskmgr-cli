using System.Drawing;
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
        bool enabled,
        ISystemTerminal terminal)
    {
        terminal.BackgroundColor = theme.Background;
        terminal.ForegroundColor = enabled ? theme.ForegroundHighlight : ConsoleColor.DarkGray;
        terminal.Write(keyBinding + " ");
        int nchars = keyBinding.Length + 1;
        
        terminal.BackgroundColor = theme.CommandBackground;
        terminal.ForegroundColor = enabled ? theme.CommandForeground : ConsoleColor.DarkGray;
        terminal.Write(text.CentreWithLength(width).ToBold());
        nchars += width;
        
        return nchars;
    }
}
