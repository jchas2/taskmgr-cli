using Task.Manager.Configuration;
using Task.Manager.Gui.Controls;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Controls;
using Task.Manager.System.Process;

namespace Task.Manager.Gui;

public sealed class MainWindow : Control
{
    private readonly IProcesses _processor;
    private readonly RunContext _runContext;
    private readonly Theme _theme;

    private readonly ProcessControl _processControl;
    
    public MainWindow(
        RunContext runContext,
        ISystemTerminal terminal,
        Theme theme,
        Config config)
    : base(terminal)
    {
        _runContext = runContext ?? throw new ArgumentNullException(nameof(runContext));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));

        _processor = new Processes();
        
        _processControl = new ProcessControl(
            terminal, 
            _processor, 
            _runContext.SystemInfo,
            _theme);
        
        Controls.Add(_processControl);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        _processControl.Show();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }
}
