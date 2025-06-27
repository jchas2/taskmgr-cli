using Task.Manager.System;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public class HelpScreen : Screen
{
    public HelpScreen(SystemTerminal terminal) : base(terminal) { }

    protected override void OnDraw()
    {
        Terminal.SetCursorPosition(X, Y);
        Terminal.Clear();
        Terminal.WriteLine("Task manager Help Screen");
    }

    protected override void OnLoad()
    {
    }

    protected override void OnResize()
    {
        X = 0;
        Y = 0;
        Width = Terminal.WindowWidth;
        Height = Terminal.WindowHeight;
    }
}