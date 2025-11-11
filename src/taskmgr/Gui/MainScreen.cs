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

        headerControl = new HeaderControl(
            this.runContext.Processor,
            terminal,
            this.theme,
            this.config) {
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
        
        commandControl.AddCommand(ConsoleKey.F1, () => new HelpCommand("Help", screenApp));
        commandControl.AddCommand(ConsoleKey.F2, () => new SetupCommand("Setup", screenApp));
        commandControl.AddCommand(ConsoleKey.F3, () => new ProcessSortCommand("Sort", this));
        commandControl.AddCommand(ConsoleKey.F4, () => new FilterCommand("Filter", this));
        commandControl.AddCommand(ConsoleKey.F5, () => new ProcessInfoCommand("Info", this));
        commandControl.AddCommand(ConsoleKey.F6, () => new EndTaskCommand("End Task", this));
        commandControl.AddCommand(ConsoleKey.F7, () => new AboutCommand("About", this));
        
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
        
        if (keyInfo.Key == ConsoleKey.Escape && activeControl != processControl) {
            SetActiveControl<ProcessControl>();
            Draw();
            handled = true;
            return;
        }
        
        activeControl.KeyPressed(keyInfo, ref handled);

        if (handled) {
            return;
        }

        commandControl.KeyPressed(keyInfo, ref handled);
    }

    protected override void OnLoad()
    {
        activeControl = processControl;

        headerControl.Load();
        activeControl.Load();
        footerControl.Load();
        
        processControl.ProcessItemSelected += OnProcessItemSelected;
    }

    private void OnProcessItemSelected(object? sender, ListViewItemEventArgs e)
    {
        // Send the F5 key to the command control to invoke the Process Info Command.
        ConsoleKeyInfo keyInfo = new(
            (char)ConsoleKey.F5, 
            ConsoleKey.F5, 
            shift: false, 
            alt: false, 
            control: false);
        
        bool handled = false;        
        commandControl.KeyPressed(keyInfo, ref handled);
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

    protected override void OnShown()
    {
        base.OnShown();
        runContext.Processor.Run();
    }

    protected override void OnUnload()
    {
        runContext.Processor.Stop();
        processControl.ProcessItemSelected -= OnProcessItemSelected;

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
