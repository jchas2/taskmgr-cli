using Task.Manager.System;
using Task.Manager.System.Controls;

namespace Task.Manager.Gui.Controls;

public sealed class FooterControl : Control
{
    public FooterControl(ISystemTerminal terminal) : base(terminal) { }

    protected override void OnDraw()
    {
        string line1 = "F1 PROCESSES  F2 MODULES  F3 THREADS  ESC EXIT";
        Terminal.SetCursorPosition(left: X, top: Y);
        Terminal.Write(line1);
        Terminal.WriteEmptyLineTo(Width - line1.Length);
    }
}