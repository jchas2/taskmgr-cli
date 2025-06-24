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
    private readonly FooterControl _footerControl;
    private Control? _activeControl;
    
    private readonly Dictionary<Type, AbstractCommand> _commandMap;

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
        
        _processControl = new ProcessControl(
            terminal, 
            _runContext.Processor, 
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
    
    public void SetActiveControl<T>() where T : Control
    {
        Debug.Assert(_activeControl != null);
        
        var nextControl = Controls.ToList().SingleOrDefault(c => c.GetType() == typeof(T));
        
        if (nextControl == null) {
            throw new InvalidOperationException();
        }
        
        _activeControl?.Unload();
        _activeControl = nextControl;
        
        SizeControl(_activeControl);
        
        _activeControl!.Load();
    }

    protected override void OnDraw()
    {
        Debug.Assert(_activeControl != null);
        
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
        SizeControl(_processControl);
        SizeControl(_modulesControl);
        SizeControl(_threadsControl);
        
        _footerControl.X = 0;
        _footerControl.Y = Terminal.WindowHeight - FooterHeight;
        _footerControl.Width = Terminal.WindowWidth;
        _footerControl.Height = FooterHeight;
        
        _runContext.Processor.Run();
        
        _activeControl = _processControl;
        _activeControl.Load();
    }

    protected override void OnResize()
    {
        foreach (var control in Controls) {
            SizeControl(control);            
        }

        _footerControl.Y = Terminal.WindowHeight - FooterHeight;
        _footerControl.Width = Terminal.WindowWidth;
        
        OnDraw();
    }

    protected override void OnUnload()
    {
        foreach (var control in Controls) {
            control.Unload();
        }
    }

    private void SizeControl(Control control)
    {
        control.X = 0;
        control.Y = 0;
        control.Width = Terminal.WindowWidth;
        control.Height = Terminal.WindowHeight - FooterHeight;
    }
}
