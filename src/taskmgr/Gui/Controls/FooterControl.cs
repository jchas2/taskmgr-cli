﻿using System.Text;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;

namespace Task.Manager.Gui.Controls;

public sealed class FooterControl : Control
{
    private Theme _theme;
    private const string CommandText = "          Help      Setup     Processes Modules   Threads   ";

    public FooterControl(ISystemTerminal terminal, Theme theme) : base(terminal) => _theme = theme;

    protected override void OnDraw()
    {
        Terminal.SetCursorPosition(left: X, top: Y);
        int nchars = 0;
        
        for (int i = 1; i <= 5; i++) {
            Terminal.BackgroundColor = _theme.Background;
            Terminal.ForegroundColor = _theme.Foreground;
            string funcKey = $"F{i} ";
            Terminal.Write(funcKey);
            nchars += funcKey.Length;
            string slice = CommandText.Substring(i * 10, 10);
            Terminal.BackgroundColor = _theme.BackgroundHighlight;
            Terminal.ForegroundColor = _theme.ForegroundHighlight;
            Terminal.Write(slice);
            nchars += slice.Length;
        }
        
        Terminal.WriteEmptyLineTo(Width - nchars);
    }
}