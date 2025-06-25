using System.Diagnostics;
using Task.Manager.Commands;
using Task.Manager.Configuration;
using Task.Manager.Gui.Controls;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Controls;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public sealed class MainScreen : Screen
{
    private readonly RunContext _runContext;
    private readonly Theme _theme;
    
    private readonly ProcessControl _processControl;
    private readonly ModulesControl _modulesControl;
    private readonly ThreadsControl _threadsControl;
    private readonly HeaderControl _headerControl;
    private readonly FooterControl _footerControl;
    private Control? _activeControl;
    
    private readonly Dictionary<Type, AbstractCommand> _commandMap;

    private const int HeaderHeight = 9;
    private const int FooterHeight = 1;
    
    public MainScreen(
        RunContext runContext,
        ISystemTerminal terminal,
        Theme theme,
        Config config)
    : base(terminal)
    {
        _runContext = runContext ?? throw new ArgumentNullException(nameof(runContext));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));

        _commandMap = new Dictionary<Type, AbstractCommand>() {
            [typeof(ProcessesCommand)] = new ProcessesCommand(this),
            [typeof(ModulesCommand)] = new ModulesCommand(this),
            [typeof(ThreadsCommand)] = new ThreadsCommand(this)
        };

        _headerControl = new HeaderControl(
            _runContext.Processor,
            terminal,
            theme);
        
        _processControl = new ProcessControl(
            _runContext.Processor,
            terminal, 
            _theme);
        
        _modulesControl = new ModulesControl(terminal, _theme);
        _threadsControl = new ThreadsControl(terminal, _theme);
        _footerControl = new FooterControl(terminal);

        Controls.Add(_processControl);
        Controls.Add(_modulesControl);
        Controls.Add(_threadsControl);
    }

    public Control? GetActiveControl => _activeControl;

    private AbstractCommand GetCommandInstance<T>() where T : AbstractCommand => _commandMap[typeof(T)];
    
    protected override void OnDraw()
    {
        Debug.Assert(_activeControl != null);
        
        _headerControl.Draw();
        _activeControl.Draw();
        _footerControl.Draw();
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo)
    {
        AbstractCommand? command = keyInfo.Key switch {
            ConsoleKey.F1 => GetCommandInstance<ProcessesCommand>(),
            ConsoleKey.F2 => GetCommandInstance<ModulesCommand>(),
            ConsoleKey.F3 => GetCommandInstance<ThreadsCommand>(),
            _ => null
        };

        if (command != null && command.IsEnabled) {
            command.Execute();
            return;
        }
        
        var activeControl = GetActiveControl;
        activeControl?.KeyPressed(keyInfo);
    }

    protected override void OnLoad()
    {
        _activeControl = _processControl;

        _headerControl.Load();
        _activeControl.Load();
        _footerControl.Load();
        
        _runContext.Processor.Run();
    }
    
    protected override void OnResize()
    {
        X = 0;
        Y = 0;
        Height = Terminal.WindowHeight;
        Width = Terminal.WindowWidth;
        
        _headerControl.X = 0;
        _headerControl.Y = 0;
        _headerControl.Height = HeaderHeight;
        _headerControl.Width = Width;
        
        foreach (var control in Controls) {
            SizeControl(control);            
        }

        _footerControl.X = 0;
        _footerControl.Y = Terminal.WindowHeight - FooterHeight;
        _footerControl.Width = Terminal.WindowWidth;
        _footerControl.Height = FooterHeight;
    }

    protected override void OnUnload()
    {
        foreach (var control in Controls) {
            control.Unload();
        }
        
        _runContext.Processor.Stop();
    }

    public Control SetActiveControl<T>() where T : Control
    {
        Debug.Assert(_activeControl != null);
        
        var nextControl = Controls.ToList().SingleOrDefault(c => c.GetType() == typeof(T));
        
        if (nextControl == null) {
            throw new InvalidOperationException();
        }
        
        _activeControl.Unload();
        _activeControl = nextControl;
        
        SizeControl(_activeControl);
        
        _activeControl.Load();
        _activeControl.Resize();

        return _activeControl;
    }

    private void SizeControl(Control control)
    {
        control.X = 0;
        control.Y = HeaderHeight;
        control.Width = Terminal.WindowWidth;
        control.Height = Terminal.WindowHeight - HeaderHeight - FooterHeight;
    }
}
