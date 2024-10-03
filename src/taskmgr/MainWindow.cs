using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Process;
using Task.Manager.Views;

namespace Task.Manager;

public sealed class MainWindow
{
    private readonly SystemHeader _systemHeader;
    private readonly IProcesses _processes;
    private readonly ISystemTerminal _terminal;
    private readonly Theme _theme;

    public MainWindow(
        SystemHeader systemHeader,
        IProcesses processes,
        ISystemTerminal terminal,
        Theme theme)
    {
        _systemHeader = systemHeader;
        _processes = processes;
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }
}
