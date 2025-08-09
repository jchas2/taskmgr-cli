using System.Diagnostics;
using Task.Manager.Commands;
using Task.Manager.Configuration;
using Task.Manager.Gui.Controls;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.InputBox;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public sealed class MainScreen : Screen
{
    private readonly RunContext _runContext;
    private readonly Theme _theme;
    private readonly Config _config;

    private readonly ProcessControl _processControl;
    private readonly ProcessInfoControl _processInfoControl;
    private readonly HeaderControl _headerControl;
    private readonly CommandControl _commandControl;
    private readonly FilterControl _filterControl;
    private Control _activeControl;
    private Control _footerControl;
    
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
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _commandMap = new Dictionary<Type, AbstractCommand>() {
            [typeof(HelpCommand)] = new HelpCommand(),
            [typeof(SetupCommand)] = new SetupCommand(),
            [typeof(ProcessSortCommand)] = new ProcessSortCommand(this),
            [typeof(FilterCommand)] = new FilterCommand(this),
            [typeof(ProcessInfoCommand)] = new ProcessInfoCommand(this),
            [typeof(EndTaskCommand)] = new EndTaskCommand(this)
        };

        _headerControl = new HeaderControl(
            _runContext.Processor,
            terminal,
            theme);
        
        _processControl = new ProcessControl(
            _runContext.Processor,
            terminal, 
            _theme);
        
        _processInfoControl = new ProcessInfoControl(terminal, _theme);
        
        _commandControl = new CommandControl(terminal, _theme);
        _filterControl = new FilterControl(terminal, _theme);

        _activeControl = _processControl;
        _footerControl = _commandControl;

        Controls
            .Add(_processControl)
            .Add(_processInfoControl);
    }

    public Control? GetActiveControl => _activeControl;

    private AbstractCommand GetCommandInstance<T>() where T : AbstractCommand => _commandMap[typeof(T)];
    
    protected override void OnDraw()
    {
        Debug.Assert(_activeControl != null);

        Terminal.CursorVisible = false;
        
        _headerControl.Draw();
        _activeControl.Draw();
        _footerControl.Draw();
        
        Terminal.CursorVisible = true;
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        base.OnKeyPressed(keyInfo, ref handled);
        
        if (handled) {
            return;
        }
        
        Control? activeControl = GetActiveControl;

        if (keyInfo.Key == ConsoleKey.Escape && activeControl != _processControl) {
            SetActiveControl<ProcessControl>();
            Draw();
            handled = true;
            return;
        }
        
        activeControl?.KeyPressed(keyInfo, ref handled);

        if (handled) {
            return;
        }
        
        AbstractCommand? command = keyInfo.Key switch {
            ConsoleKey.F1 => GetCommandInstance<HelpCommand>(),
            ConsoleKey.F2 => GetCommandInstance<SetupCommand>(),
            ConsoleKey.F3 => GetCommandInstance<ProcessSortCommand>(),
            ConsoleKey.F4 => GetCommandInstance<FilterCommand>(),
            ConsoleKey.F5 => GetCommandInstance<ProcessInfoCommand>(),
            ConsoleKey.F6 => GetCommandInstance<EndTaskCommand>(),
            _ => null
        };

        if (command != null && command.IsEnabled) {
            command.Execute();
            handled = true;
        }
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
        base.OnResize();
        
        Clear();
        
        _headerControl.X = 0;
        _headerControl.Y = 0;
        _headerControl.Height = HeaderHeight;
        _headerControl.Width = Width;
        _headerControl.Resize();
        
        foreach (Control control in Controls) {
            SizeControl(control);            
        }

        _footerControl.X = 0;
        _footerControl.Y = Height - FooterHeight;
        _footerControl.Width = Width;
        _footerControl.Height = FooterHeight;
        _footerControl.Resize();
    }

    protected override void OnUnload()
    {
        _runContext.Processor.Stop();

        foreach (Control control in Controls) {
            control.Unload();
        }
    }

    public T SetActiveControl<T>() where T : Control
    {
        Debug.Assert(_activeControl != null);
        
        Control? nextControl = Controls.ToList().SingleOrDefault(c => c.GetType() == typeof(T));
        
        if (nextControl == null) {
            throw new InvalidOperationException();
        }
        
        _activeControl.Unload();
        _activeControl = nextControl;
        
        SizeControl(_activeControl);
        
        _activeControl.Load();
        _activeControl.Resize();

        return (T)_activeControl;
    }

    public void ShowCommandControl()
    {
        _filterControl.Visible = false;
        ShowFooterControl(_commandControl);   
    }

    public void ShowFilterControl(Action<string, InputBoxResult> onInputBoxResult)
    {
        _commandControl.Visible = false;
         ShowFooterControl(_filterControl);
        
        ShowInputBox(
            _filterControl.X + _filterControl.NeededWidth,
            _filterControl.Y,
            40,
            "Filter: ",
            onInputBoxResult);
    }

    private void ShowFooterControl(Control control)
     {
        _footerControl = control;
        _footerControl.Visible = true;
        
        Resize();
        Draw();
    }
    
    private void SizeControl(Control control)
    {
        control.X = 0;
        control.Y = HeaderHeight;
        control.Width = Width;
        control.Height = Height - HeaderHeight - FooterHeight;
        control.Resize();
    }
}
