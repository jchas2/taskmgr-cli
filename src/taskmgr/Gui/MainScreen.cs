using System.Diagnostics;
using Task.Manager.Commands;
using Task.Manager.Configuration;
using Task.Manager.Gui.Controls;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public sealed class MainScreen : Screen
{
    private readonly RunContext _runContext;
    private readonly Theme _theme;
    private readonly Config _config;

    private readonly ListView _menuView;
    private readonly ProcessControl _processControl;
    private readonly ProcessInfoControl _processInfoControl;
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
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _commandMap = new Dictionary<Type, AbstractCommand>() {
            [typeof(HelpCommand)] = new HelpCommand(),
            [typeof(SetupCommand)] = new SetupCommand(),
            [typeof(ProcessSortCommand)] = new ProcessSortCommand(this),
            [typeof(ProcessInfoCommand)] = new ProcessInfoCommand(this)
        };

        _menuView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = true
        };
        
        _menuView.ColumnHeaders.Add(new ListViewColumnHeader("MENU"));

        _headerControl = new HeaderControl(
            _runContext.Processor,
            terminal,
            theme);
        
        _processControl = new ProcessControl(
            _runContext.Processor,
            terminal, 
            _theme);
        
        _processInfoControl = new ProcessInfoControl(terminal, _theme);
        _footerControl = new FooterControl(terminal, _theme);

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
        AbstractCommand? command = keyInfo.Key switch {
            ConsoleKey.F1 => GetCommandInstance<HelpCommand>(),
            ConsoleKey.F2 => GetCommandInstance<SetupCommand>(),
            ConsoleKey.F3 => GetCommandInstance<ProcessSortCommand>(),
            ConsoleKey.F4 => GetCommandInstance<ProcessInfoCommand>(),
            _ => null
        };

        if (command != null && command.IsEnabled) {
            command.Execute();
            handled = true;
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
    
    private void SizeControl(Control control)
    {
        control.X = 0;
        control.Y = HeaderHeight;
        control.Width = Width;
        control.Height = Height - HeaderHeight - FooterHeight;
        control.Resize();
    }
}
