using Task.Manager.Configuration;
using Task.Manager.Gui.Controls;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Controls.MessageBox;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public class SetupScreen : Screen
{
    private readonly RunContext runContext;
    private readonly ListView menuView;
    private readonly ListView themeView;
    private readonly ListView metreView;
    private readonly ListView delayView;
    private readonly ListView limitView;
    private readonly ListView numProcsView;
    private readonly List<ListView> tabControls = [];

    private ListView focusedControl; 
    private Theme previewTheme;

    private const int ControlGutter = 1;
    private const int MenuViewWidth = 22;
    private const int CommandLength = 10;

    public SetupScreen(RunContext runContext) : base(runContext.Terminal)
    {
        this.runContext = runContext;
        
        menuView = new(runContext.Terminal);
        menuView.ColumnHeaders.Add(new ListViewColumnHeader("CATEGORIES"));

        themeView = new(runContext.Terminal) {
            ShowColumnHeaders = true,
            EnableScroll = true,
            Visible = false
        };

        themeView.ColumnHeaders.Add(new ListViewColumnHeader("Available themes"));

        metreView = new(runContext.Terminal) {
            ShowColumnHeaders = true,
            EnableScroll = true,
            Visible = false
        };

        metreView.ColumnHeaders.Add(new ListViewColumnHeader("Metre Styles"));
        
        delayView = new(runContext.Terminal) {
            ShowColumnHeaders = true,
            EnableScroll = true,
            Visible = false
        };

        delayView.ColumnHeaders.Add(new ListViewColumnHeader("Delay between updates, in milliseconds"));
        
        limitView = new(runContext.Terminal) {
            ShowColumnHeaders = true,
            EnableScroll = true,
            Visible = false
        };

        limitView.ColumnHeaders.Add(new ListViewColumnHeader("Limit the number of process iterations, 0 = loop forever"));
        
        numProcsView = new(runContext.Terminal) {
            ShowColumnHeaders = true,
            EnableScroll = true,
            Visible = false
        };

        numProcsView.ColumnHeaders.Add(new ListViewColumnHeader("Number of processes to display, -1 for all"));

        Controls
            .Add(menuView)
            .Add(themeView)
            .Add(metreView)
            .Add(delayView)
            .Add(limitView)
            .Add(numProcsView);
        
        tabControls.AddRange(new [] {
            themeView, 
            metreView,
            delayView, 
            limitView,
            numProcsView
        });

        focusedControl = menuView;
        previewTheme = runContext.AppConfig.DefaultTheme;
    }

    private void LoadMenuItems()
    {
        menuView.Items.Add(
            new MenuListViewItem(
                themeView, 
                "THEMES"));
        
        menuView.Items.Add(
            new MenuListViewItem(
                metreView,
                "METRES"));

        menuView.Items.Add(
            new MenuListViewItem(
                delayView,
                "DELAY"));

        menuView.Items.Add(
            new MenuListViewItem(
                limitView,
                "LIMIT"));

        menuView.Items.Add(
            new MenuListViewItem(
                numProcsView,
                "PROCESSES"));
    }
    
    private void LoadSectionConfigListView<T>(
        ListView listView,
        List<T> values,
        T value)
    {
        int index = values.BinarySearch(value);

        if (index < 0) {
            values.Insert(-index, value);
        }
        
        for (int i = 0; i < values.Count; i++) {
            listView.Items.Add(new ListViewItem(values[i]?.ToString() ?? string.Empty));
            
            if (values[i]!.Equals(value)) {
                listView.SelectedIndex = i;
            }
        }
    }

    private void LoadUxSection()
    {
        void AddItems(ListView listView, List<string> items, Func<string, bool> func)
        {
            for (int i = 0; i < items.Count; i++) {
                listView.Items.Add(new ListViewItem(items[i]));

                if (func(items[i])) {
                    listView.SelectedIndex = i;
                }
            }
        }
        
        List<string> themeNames = runContext.AppConfig.Themes
            .Where(t => t.Name.StartsWith("theme-", StringComparison.CurrentCultureIgnoreCase))
            .Select(t => t.Name)
            .ToList();

        AddItems(themeView, themeNames, val => runContext.AppConfig.DefaultTheme.Name.Equals(val));
        
        List<string> metreStyles = Enum.GetValues<MetreControlStyle>()
            .Select(c => c.ToString())
            .ToList();

        AddItems(metreView, metreStyles, val => runContext.AppConfig.MetreStyle.ToString().Equals(val));
    }

    private void MapControlsToConfig()
    {
        void UpdateConfigValue(ListViewItem? sourceItem, Action<int> action)
        {
            if (sourceItem?.Text != null) {
                action(int.Parse(sourceItem.Text));
            }
        }

        if (themeView.SelectedItem?.Text != null) {
            runContext.AppConfig.DefaultTheme = runContext.AppConfig.Themes.First(
                t => t.Name.Equals(themeView.SelectedItem.Text, StringComparison.CurrentCultureIgnoreCase));
        }
        
        runContext.AppConfig.MetreStyle = Enum.GetValues<MetreControlStyle>()
            .Single(c => c.ToString() == metreView.SelectedItem?.Text);

        UpdateConfigValue(delayView.SelectedItem, val => runContext.AppConfig.DelayInMilliseconds = val);
        UpdateConfigValue(limitView.SelectedItem, val => runContext.AppConfig.IterationLimit = val);
        UpdateConfigValue(numProcsView.SelectedItem, val => runContext.AppConfig.NumberOfProcesses = val);
    }
    
    private void MenuViewOnItemClicked(object? sender, ListViewItemEventArgs e)
    {
        tabControls.ForEach(ctrl => ctrl.Visible = false);
        
        var menuListViewItem = e.Item as MenuListViewItem;
        menuListViewItem!.AssociatedControl.Visible = true;
        
        Resize();
        Draw();
    }

    protected override void OnDraw()
    {
        DrawRectangle(
            X,
            Y,
            Width,
            Height,
            previewTheme.Background);

        Terminal.SetCursorPosition(X, Y);
        Terminal.BackgroundColor = previewTheme.Background;
        Terminal.ForegroundColor = previewTheme.Foreground;
        
        UpdateTheme(menuView);
        menuView.Draw();

        foreach (ListView ctrl in tabControls) {
            UpdateTheme(ctrl);
        }
        
        ListView activeControl = tabControls.Single(ctrl => ctrl.Visible);
        activeControl.Draw();

        KeyBindControl.Draw(
            "F8",
            "Save",
            X,
            Height - ControlGutter,
            CommandLength,
            previewTheme,
            enabled: true,
            Terminal);
        
        KeyBindControl.Draw(
            "Esc",
            "Exit",
            X + CommandLength,
            Height - ControlGutter,
            CommandLength,
            previewTheme,
            enabled: true,
            Terminal);
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        base.OnKeyPressed(keyInfo, ref handled);
        
        if (handled) {
            return;
        }

        ListView activeControl = tabControls.Single(ctrl => ctrl.Visible);

        switch (keyInfo.Key) {
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow:
            case ConsoleKey.PageUp:
            case ConsoleKey.PageDown:
                focusedControl.KeyPressed(keyInfo, ref handled);
                break;

            case ConsoleKey.LeftArrow:
                focusedControl = menuView;
                focusedControl.Draw();

                if (menuView.SelectedItem != null) {
                    ListViewItemEventArgs e = new(menuView.SelectedItem);
                    MenuViewOnItemClicked(this, e);
                }

                break;

            case ConsoleKey.RightArrow:
                focusedControl = activeControl;
                focusedControl.Draw();
                break;
            
            case ConsoleKey.F8:
                SaveConfig();
                break;
        }
    }

    protected override void OnLoad()
    {
        foreach (Control control in Controls) {
            control.Load();
        }
        
        foreach (ListView listView in tabControls) {
            listView.Visible = false;
        }

        LoadMenuItems();
        LoadUxSection();
        
        LoadSectionConfigListView(
            delayView,
            [ 1000, 1500, 2000, 5000, 10000 ],
            runContext.AppConfig.DelayInMilliseconds);        

        LoadSectionConfigListView(
            numProcsView,
            [ -1, 5, 10, 20, 50, 100, 500, 1000 ],
            runContext.AppConfig.NumberOfProcesses);        
        
        LoadSectionConfigListView(
            limitView,
            [ 0, 1, 3, 5, 10, 20, 50, 100, 500, 1000 ],
            runContext.AppConfig.IterationLimit);

        previewTheme = runContext.AppConfig.DefaultTheme;
        focusedControl = menuView;
        themeView.Visible = true;
        
        menuView.ItemClicked += MenuViewOnItemClicked;
        themeView.ItemClicked += ThemeViewOnItemClicked;
    }

    private void ThemeViewOnItemClicked(object? sender, ListViewItemEventArgs e)
    {
        Theme theme = runContext.AppConfig.Themes
            .Where(t => t.Name.Equals(e.Item.Text, StringComparison.CurrentCultureIgnoreCase))
            .First();

        previewTheme = theme;
        Draw();
    }

    protected override void OnResize()
    {
        base.OnResize();
        Clear();
        
        menuView.X = X;
        menuView.Y = Y;
        menuView.Height = Height - ControlGutter;
        menuView.Width = MenuViewWidth;
        menuView.ColumnHeaders[0].Width = MenuViewWidth;
        menuView.Resize();
        
        foreach (ListView ctrl in tabControls) {
            ctrl.X = menuView.X + menuView.Width + ControlGutter;
            ctrl.Y = menuView.Y;
            ctrl.Height = menuView.Height;
            ctrl.Width = Width - (menuView.Width + ControlGutter);
            ctrl.ColumnHeaders[0].Width = ctrl.Width;
            ctrl.Resize();
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        
        menuView.ItemClicked -= MenuViewOnItemClicked;
        themeView.ItemClicked -= ThemeViewOnItemClicked;

        menuView.Items.Clear();
        
        foreach (Control control in Controls) {
            control.Unload();
            (control as ListView)?.Items.Clear();
        }
        
        focusedControl = menuView;
    }

    private void SaveConfig()
    {
        string msg = $"Save settings to config file:\n{runContext.AppConfig.DefaultConfigPath}?";
        
        ShowMessageBox(
            "Save Config",
            msg,
            MessageBoxButtons.OkCancel,
            () => {
                MapControlsToConfig();
                
                if (runContext.AppConfig.TrySave(runContext.AppConfig.DefaultConfigPath ?? string.Empty)) {
                    ShowMessageBox(
                        "Save Succeeded",
                        "Config file successfully saved.",
                        MessageBoxButtons.Ok,
                        () => { });
                }
                else {
                    ShowMessageBox(
                        "Save Failed",
                        "An error occurred saving config file.",
                        MessageBoxButtons.Ok,
                        () => { });
                }
            },
            msg.Length + ControlGutter * 2);
    }

    private void UpdateTheme(ListView listView)
    {
        listView.BackgroundHighlightColour = previewTheme.BackgroundHighlight;
        listView.ForegroundHighlightColour = previewTheme.ForegroundHighlight;
        listView.BackgroundColour = previewTheme.Background;
        listView.ForegroundColour = previewTheme.Foreground;
        listView.HeaderBackgroundColour = previewTheme.HeaderBackground;
        listView.HeaderForegroundColour = previewTheme.HeaderForeground;
    } 
}
