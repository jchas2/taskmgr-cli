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
    private readonly RunContext runContext;
    private readonly Theme theme;
    private readonly Config config;

    private readonly ProcessControl processControl;
    private readonly ProcessInfoControl processInfoControl;
    private readonly HeaderControl headerControl;
    private readonly CommandControl commandControl;
    private readonly FilterControl filterControl;
    private Control activeControl;
    private Control footerControl;
    
    private readonly Dictionary<Type, AbstractCommand> commandMap;

    private const int HeaderHeight = 9;
    private const int FooterHeight = 1;
    
    public MainScreen(
        ScreenApplication screenApp,
        RunContext runContext,
        ISystemTerminal terminal,
        Theme theme,
        Config config)
    : base(terminal)
    {
        this.runContext = runContext ?? throw new ArgumentNullException(nameof(runContext));
        this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
        this.config = config ?? throw new ArgumentNullException(nameof(config));

        commandMap = new Dictionary<Type, AbstractCommand>() {
            [typeof(HelpCommand)]        = new HelpCommand(screenApp),
            [typeof(SetupCommand)]       = new SetupCommand(screenApp),
            [typeof(ProcessSortCommand)] = new ProcessSortCommand(this),
            [typeof(FilterCommand)]      = new FilterCommand(this),
            [typeof(ProcessInfoCommand)] = new ProcessInfoCommand(this),
            [typeof(EndTaskCommand)]     = new EndTaskCommand(this)
        };

        headerControl = new HeaderControl(
            this.runContext.Processor,
            terminal,
            theme) {
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground
        };

        processControl = new ProcessControl(
            this.runContext.Processor,
            terminal,
            this.theme,
            this.config) {
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground
        };

        processInfoControl = new ProcessInfoControl(
            this.runContext.ProcessService,
            this.runContext.ModuleService,
            this.runContext.ThreadService,
            terminal, 
            this.theme) {
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground
        };

        commandControl = new CommandControl(terminal, this.theme) {
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground
        };


        filterControl = new FilterControl(terminal, this.theme) {
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground
        };

        activeControl = processControl;
        footerControl = commandControl;

        Controls
            .Add(processControl)
            .Add(processInfoControl);
    }

    public Control? GetActiveControl => activeControl;

    public T GetControl<T>() where T : Control => (T)Controls.Single(ctrl => ctrl is T);
    
    private AbstractCommand GetCommandInstance<T>() where T : AbstractCommand => commandMap[typeof(T)];
    
    protected override void OnDraw()
    {
        Debug.Assert(activeControl != null);

        Terminal.CursorVisible = false;
        
        headerControl.Draw();
        activeControl.Draw();
        footerControl.Draw();
        
        Terminal.CursorVisible = true;
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        base.OnKeyPressed(keyInfo, ref handled);
        
        if (handled) {
            return;
        }
        
        Control? activeControl = GetActiveControl;

        if (keyInfo.Key == ConsoleKey.Escape && activeControl != processControl) {
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
        activeControl = processControl;

        headerControl.Load();
        activeControl.Load();
        footerControl.Load();
        
        runContext.Processor.Run();
    }
    
    protected override void OnResize()
    {
        base.OnResize();
        
        Clear();
        
        headerControl.X = 0;
        headerControl.Y = 0;
        headerControl.Height = HeaderHeight;
        headerControl.Width = Width;
        headerControl.Resize();
        
        foreach (Control control in Controls) {
            SizeControl(control);            
        }

        footerControl.X = 0;
        footerControl.Y = Height - FooterHeight;
        footerControl.Width = Width;
        footerControl.Height = FooterHeight;
        footerControl.Resize();
    }

    protected override void OnUnload()
    {
        runContext.Processor.Stop();

        foreach (Control control in Controls) {
            control.Unload();
        }
    }

    public T SetActiveControl<T>() where T : Control
    {
        Debug.Assert(activeControl != null);
        
        Control? nextControl = Controls.ToList().SingleOrDefault(c => c.GetType() == typeof(T));
        
        if (nextControl == null) {
            throw new InvalidOperationException();
        }
        
        activeControl.Unload();
        activeControl = nextControl;
        
        SizeControl(activeControl);
        
        activeControl.Load();
        activeControl.Resize();

        return (T)activeControl;
    }

    public void ShowCommandControl()
    {
        filterControl.Visible = false;
        ShowFooterControl(commandControl);   
    }

    public void ShowFilterControl(Action<string, InputBoxResult> onInputBoxResult)
    {
        commandControl.Visible = false;
        ShowFooterControl(filterControl);
        
        ShowInputBox(
            filterControl.X + filterControl.NeededWidth,
            filterControl.Y,
            40,
            "Filter: ",
            onInputBoxResult);
    }

    private void ShowFooterControl(Control control)
     {
        footerControl = control;
        footerControl.Visible = true;
        
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
