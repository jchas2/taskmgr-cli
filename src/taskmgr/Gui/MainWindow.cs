using Task.Manager.Configuration;
using Task.Manager.Gui.Controls;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Controls;
using Task.Manager.System.Process;

namespace Task.Manager.Gui;

public sealed class MainWindow : Control
{
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

        _processControl = new ProcessControl(
            terminal, 
            _runContext.Processor, 
            _runContext.SystemInfo,
            _theme);

        Controls.Add(_processControl);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        _processControl.MarginBottom = 0;
        _processControl.MarginLeft = 0;
        _processControl.MarginRight = 0;
        _processControl.MarginTop = 0;
        
        _runContext.OutputWriter.WriteLine("Loading processor...");
        _runContext.Processor.Run();

        _runContext.OutputWriter.WriteLine("Loading TUI...");
        _processControl.Show();

        Thread.CurrentThread.Join();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
    }
}
