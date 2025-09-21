using Task.Manager.System;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public class HelpScreen : Screen
{
    public HelpScreen(SystemTerminal terminal) : base(terminal) { }

    protected override void OnDraw()
    {
        DrawRectangle(
            X,
            Y,
            Width,
            Height,
            BackgroundColour);

        Terminal.SetCursorPosition(X, Y);
        Terminal.WriteLine("Task manager Help Screen");
    }

    protected override void OnLoad()
    {
    }
}