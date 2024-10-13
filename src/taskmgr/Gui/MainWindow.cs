using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Process;

namespace Task.Manager.Gui;

public sealed class MainWindow
{
    private readonly SystemHeaderView _systemHeaderView;
    private readonly IProcesses _processes;
    private readonly ISystemTerminal _terminal;
    private readonly Theme _theme;

    public MainWindow(
        SystemHeaderView systemHeaderView,
        IProcesses processes,
        ISystemTerminal terminal,
        Theme theme)
    {
        _systemHeaderView = systemHeaderView;
        _processes = processes;
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }
}
