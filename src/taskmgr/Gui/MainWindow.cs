using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Process;

namespace Task.Manager.Gui;

public sealed class MainWindow : Control
{
    private readonly SystemHeaderView _systemHeaderView;
    private readonly IProcesses _processes;
    private readonly Theme _theme;

    public MainWindow(
        SystemHeaderView systemHeaderView,
        IProcesses processes,
        ISystemTerminal terminal,
        Theme theme)
    : base(terminal)
    {
        _systemHeaderView = systemHeaderView;
        _processes = processes;
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
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
