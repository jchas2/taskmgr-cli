using Task.Manager.System;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public class SetupScreen : Screen
{
    public SetupScreen(SystemTerminal terminal) : base(terminal) { }

    protected override void OnDraw()
    {
        Terminal.SetCursorPosition(X, Y);
        Terminal.Clear();
        Terminal.WriteLine("Task manager Setup Screen");
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