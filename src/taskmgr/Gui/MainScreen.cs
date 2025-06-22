using System.Diagnostics;
using Task.Manager.Commands;
using Task.Manager.Configuration;
using Task.Manager.Gui.Controls;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Controls;
using Task.Manager.System.Process;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public sealed class MainScreen : Screen
{
    private readonly RunContext _runContext;
    private readonly Theme _theme;

    private readonly ProcessControl _processControl;
    private readonly ModulesControl _modulesControl;
    private Control? _activeControl;

    private readonly Dictionary<Type, AbstractCommand> _commandMap;
    
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
            [typeof(ModulesCommand)] = new ModulesCommand(this)
        };
        
        _processControl = new ProcessControl(
            terminal, 
            _runContext.Processor, 
            _theme);

        _modulesControl = new ModulesControl(terminal, _theme);

        Controls.Add(_processControl);
        Controls.Add(_modulesControl);
    }

    public Control? GetActiveControl => _activeControl;

    private AbstractCommand GetCommandInstance<T>() where T : AbstractCommand => _commandMap[typeof(T)];
    
    public void SetActiveControl<T>() where T : Control
    {
        var nextControl = Controls.ToList().SingleOrDefault(c => c.GetType() == typeof(T));
        
        if (nextControl == null) {
            throw new InvalidOperationException();
        }
        
        _activeControl?.Unload();
        _activeControl = nextControl;
        _activeControl!.Load();
    }

    protected override void OnDraw()
    {
        _processControl.Draw();
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo)
    {
        AbstractCommand? command = keyInfo.Key switch {
            ConsoleKey.F1 => GetCommandInstance<ProcessesCommand>(),
            ConsoleKey.F2 => GetCommandInstance<ModulesCommand>(),
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
        _processControl.X = 0;
        _processControl.Y = 0;
        _processControl.Width = Terminal.WindowWidth;
        _processControl.Height = Terminal.WindowHeight - 2;

        _modulesControl.X = 0;
        _modulesControl.Y = 0;
        _modulesControl.Width = Terminal.WindowWidth;
        _modulesControl.Height = Terminal.WindowHeight - 2;
        
        _runContext.OutputWriter.WriteLine("Loading processor...");
        _runContext.Processor.Run();

        _runContext.OutputWriter.WriteLine("Loading TUI...");
        _processControl.Load();
        
        _activeControl = _processControl;
    }

    protected override void OnResize()
    {
        Debug.Assert(_activeControl == null);

        foreach (var control in Controls) {
            control.Width = Terminal.WindowWidth;
            control.Height = Terminal.WindowHeight - 2;
        }
        
        _activeControl?.Draw();
    }

    protected override void OnUnload()
    {
        foreach (var control in Controls) {
            control.Unload();
        }
    }
}
