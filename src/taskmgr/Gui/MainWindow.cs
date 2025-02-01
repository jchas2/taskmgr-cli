using Task.Manager.Configuration;
using Task.Manager.Gui.Controls;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Process;

namespace Task.Manager.Gui;

public sealed class MainWindow : Control
{
    private readonly HeaderControl _headerControl;
    private readonly IProcesses _processes;
    private readonly Theme _theme;

    public MainWindow(
        IProcesses processes,
        ISystemTerminal terminal,
        Theme theme)
    : base(terminal)
    {
        _processes = processes;
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        
        _headerControl = new HeaderControl(Terminal);
        Controls.Add(_headerControl);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }
}
