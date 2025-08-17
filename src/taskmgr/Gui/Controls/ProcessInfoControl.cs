using System.Diagnostics;
using System.Runtime.CompilerServices;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ProcessInfoControl : Control
{
    private readonly Theme theme;
    private readonly ListView processInfoView;
    private readonly ListView menuView;
    private readonly ListView modulesView;
    private readonly ListView threadsView;
    private readonly ListView handlesView;
    private readonly List<ListView> tabControls = [];
    private ListView focusedControl;
    private Thread? workerThread;
    private CancellationTokenSource? cancellationTokenSource;
    private int selectedProcessId = -1;

    private const int ControlGutter = 1;
    private const int ProcessInfoViewHeight = 8;
    private const int MenuViewWidth = 10;

    public ProcessInfoControl(ISystemTerminal terminal, Theme theme)
        : base(terminal)
    {
        this.theme = theme;
        
        processInfoView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            EnableScroll = false,
            EnableRowSelect = false,
            ShowColumnHeaders = false,
            Visible = true
        };

        processInfoView.ColumnHeaders
            .Add(new ListViewColumnHeader("KEY"))
            .Add(new ListViewColumnHeader("VALUE"));

        menuView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = true
        };

        menuView.ColumnHeaders.Add(new ListViewColumnHeader("SELECT"));

        threadsView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = true
        };

        threadsView.ColumnHeaders
            .Add(new ListViewColumnHeader("THREAD ID"))
            .Add(new ListViewColumnHeader("STATE"))
            .Add(new ListViewColumnHeader("REASON"))
            .Add(new ListViewColumnHeader("PRI"))
            .Add(new ListViewColumnHeader("START ADDRESS"))
            .Add(new ListViewColumnHeader("KERNEL TIME"))
            .Add(new ListViewColumnHeader("USER TIME"))
            .Add(new ListViewColumnHeader("TOTAL TIME"));
        
        modulesView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = false
        };

        modulesView.ColumnHeaders
            .Add(new ListViewColumnHeader("MODULE"))
            .Add(new ListViewColumnHeader("PATH"));

        handlesView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = false
        };

        handlesView.ColumnHeaders
            .Add(new ListViewColumnHeader("ID"))
            .Add(new ListViewColumnHeader("NAME"));

        Controls
            .Add(processInfoView)
            .Add(menuView)
            .Add(modulesView)
            .Add(threadsView)
            .Add(handlesView);
        
        tabControls.AddRange(new [] {
            modulesView, 
            threadsView, 
            handlesView
        });

        focusedControl = threadsView;
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
        try {
            Control.DrawingLockAcquire();

            processInfoView.Draw();
            menuView.Draw();
            threadsView.Draw();
            modulesView.Draw();
            handlesView.Draw();

            Control activeControl = tabControls.Single(ctrl => ctrl.Visible);
            activeControl.Draw();
        }
        finally {
            Control.DrawingLockRelease();
        }
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        try {
            Control.DrawingLockAcquire();
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
                    
                    if (activeControl.SelectedIndex <= menuView.SelectedIndex) {
                        menuView.SelectedIndex = activeControl.SelectedIndex;
                    }
                    else {
                        if (menuView.Items.Count > 0) {
                            menuView.SelectedIndex = menuView.Items.Count - 1;
                        }
                    }
                    
                    focusedControl.Draw();

                    if (menuView.SelectedItem != null) {
                        ListViewItemEventArgs e = new(menuView.SelectedItem);
                        MenuViewOnItemClicked(this, e);
                    }
                    
                    break;
                
                case ConsoleKey.RightArrow:
                    focusedControl = activeControl;
                    
                    if (focusedControl.Items.Count > menuView.Items.Count) {
                        focusedControl.SelectedIndex = menuView.SelectedIndex;
                    }
                    else {
                        if (focusedControl.Items.Count > 0) {
                            focusedControl.SelectedIndex = focusedControl.Items.Count - 1;
                        }
                    }
                    
                    focusedControl.Draw();
                    break;
            }
        }
        finally {
            Control.DrawingLockRelease();
        }
    }

    protected override void OnLoad()
    {
        foreach (Control control in Controls) {
            control.Load();
        }

        menuView.Items.Add(
            new MenuListViewItem(
                threadsView, 
                "THREADS",
                theme.Background,
                theme.Foreground));
        
        menuView.Items.Add(
            new MenuListViewItem(
                modulesView, "MODULES",
                theme.Background,
                theme.Foreground));
        
        menuView.Items.Add(
            new MenuListViewItem(
                handlesView, 
                "HANDLES",
                theme.Background,
                theme.Foreground));

        TryLoadProcessInfo();
        TryUpdateListViewThreadItems();
        TryUpdateListViewModuleItems();
        
        menuView.ItemClicked += MenuViewOnItemClicked;
        
        cancellationTokenSource = new CancellationTokenSource();

        workerThread = new Thread(() => UpdateListViewItemsLoop(cancellationTokenSource.Token));
        workerThread.Start();
    }

    protected override void OnResize()
    {
        Clear();
        
        processInfoView.X = X;
        processInfoView.Y = Y + 1;
        processInfoView.Width = Width;
        processInfoView.Height = ProcessInfoViewHeight;
        processInfoView.ColumnHeaders[(int)InfoColumns.Key].Width = ColumnInfoKeyWidth;
        processInfoView.ColumnHeaders[(int)InfoColumns.Value].Width = Width - ColumnInfoKeyWidth;
        processInfoView.Resize();
        
        menuView.X = X;
        menuView.Y = processInfoView.Y + processInfoView.Height + ControlGutter;
        menuView.Height = Height - (ProcessInfoViewHeight + 1 + ControlGutter);
        menuView.Width = MenuViewWidth;
        menuView.ColumnHeaders[0].Width = MenuViewWidth;
        menuView.Resize();

        tabControls.ForEach(ctrl => {
            ctrl.X = menuView.X + menuView.Width + ControlGutter;
            ctrl.Y = menuView.Y;
            ctrl.Height = menuView.Height;
            ctrl.Width = Width - (menuView.Width + ControlGutter); 
        });
        
        modulesView.ColumnHeaders[(int)ModuleColumns.ModuleName].Width = ColumnModuleNameWidth;
        modulesView.ColumnHeaders[(int)ModuleColumns.FileName].Width = modulesView.Width - ColumnModuleNameWidth;
        modulesView.Resize();
        
        threadsView.ColumnHeaders[(int)ThreadColumns.Id].Width = ColumnThreadIdWidth;
        threadsView.ColumnHeaders[(int)ThreadColumns.State].Width = ColumnThreadStateWidth;
        threadsView.ColumnHeaders[(int)ThreadColumns.Reason].Width = ColumnThreadReasonWidth;
        threadsView.ColumnHeaders[(int)ThreadColumns.Priority].Width = ColumnThreadPriorityWidth;
        threadsView.ColumnHeaders[(int)ThreadColumns.StartAddress].Width = ColumnThreadStartAddressWidth;
        threadsView.ColumnHeaders[(int)ThreadColumns.CpuKernelTime].Width = ColumnThreadCpuKernelTimeWidth;
        threadsView.ColumnHeaders[(int)ThreadColumns.CpuUserTime].Width = ColumnThreadCpuUserTimeWidth;
        threadsView.ColumnHeaders[(int)ThreadColumns.CpuTotalTime].Width = ColumnThreadCpuTotalTimeWidth;
        threadsView.Resize();
        
        handlesView.Resize();
    }

    protected override void OnUnload()
    {
        cancellationTokenSource?.Cancel();

        if (workerThread != null) {
            while (workerThread.IsAlive) {
                Thread.Sleep(100);
            }
        }
        
        processInfoView.Items.Clear();
        menuView.Items.Clear();
        modulesView.Items.Clear();
        threadsView.Items.Clear();
        handlesView.Items.Clear();
        
        foreach (Control control in Controls) {
            control.Unload();
        }
        
        menuView.ItemClicked -= MenuViewOnItemClicked;
    }

    public int SelectedProcessId
    {
        get => selectedProcessId;
        set => selectedProcessId = value;
    }
    
    private void TryLoadProcessInfo()
    {
        try {
            if (!ProcessUtils.TryGetProcessByPid(SelectedProcessId, out Process? process)) {
                processInfoView.Items.Clear();
                return;
            }

            processInfoView.Items.Add(
                new(["Pid:", process!.Id.ToString()],
                theme.Background,
                theme.Foreground));
            
            processInfoView.Items.Add(
                new(["File:", process!.MainModule!.ModuleName],
                theme.Background,
                theme.Foreground));
                    
            processInfoView.Items.Add(
                new(["Description:", ""],
                theme.Background,
                theme.Foreground));
                    
            processInfoView.Items.Add(
                new(["Path:", ProcessUtils.GetProcessCommandLine(process)],
                theme.Background,
                theme.Foreground));
            
            processInfoView.Items.Add(
                new(["User:", ProcessUtils.GetProcessUserName(process)],
                theme.Background,
                theme.Foreground));
            
            processInfoView.Items.Add(
                new(["Version:", ""],
                theme.Background,
                theme.Foreground));
            
            processInfoView.Items.Add(
                new(["Size:", ""],
                theme.Background,
                theme.Foreground));
            
            processInfoView.Items.Add(
                new(["", ""],
                theme.Background,
                theme.Foreground));
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex);
            processInfoView.Items.Add(new(new[] { "Error:", ex.Message.ToRed() }));
        }
    }
    
    private void TryUpdateListViewModuleItems()
    {
        try {
            List<ModuleInfo> modules = ModuleInfo.GetModules(SelectedProcessId)
                .OrderBy(m => m.ModuleName)
                .ToList();

            foreach (var moduleInfo in modules) {
                modulesView.Items.Add(new ModuleListViewItem(moduleInfo, theme));
            }
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex);
        }
    }
    
    private void TryUpdateListViewThreadItems()
    {
        try {
            Control.DrawingLockAcquire();

            List<ThreadInfo> threads = ThreadInfo.GetThreads(SelectedProcessId)
                .OrderByDescending(t => t.CpuTotalTime.Ticks)
                .ToList();

            if (threads.Count == 0) {
                threadsView.Items.Clear();
                return;
            }

            int selectedIndex = threadsView.SelectedIndex;

            for (int i = threadsView.Items.Count - 1; i >= 0; i--) {
                var item = (ThreadListViewItem)threadsView.Items[i];

                if (!threads.Any(t => t.ThreadId == item.ThreadId)) {
                    threadsView.Items.RemoveAt(i);
                }
            }

            var threadLookup = threadsView.Items.Cast<ThreadListViewItem>().ToDictionary(t => t.ThreadId);

            for (int i = 0; i < threads.Count; i++) {
                if (threadLookup.TryGetValue(threads[i].ThreadId, out var foundItem)) {
                    foundItem.UpdateItem(threads[i]);
                    int insertAt = Math.Min(i, threadsView.Items.Count - 1);
                    threadsView.Items.Remove(foundItem);
                    threadsView.Items.InsertAt(insertAt, foundItem);
                }
                else {
                    ThreadListViewItem item = new(threads[i], theme);
                    threadsView.Items.InsertAt(i, item);
                }
            }

            if (threadsView.Items.Count > 0) {
                threadsView.SelectedIndex = selectedIndex >= 0 && selectedIndex < threadsView.Items.Count
                    ? selectedIndex
                    : 0;
            }
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex);
        }
        finally {
            Control.DrawingLockRelease();
        }
    }
    
    private void UpdateListViewItemsLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested) {
            TryUpdateListViewThreadItems();
            Draw();
            Thread.Sleep(1000);
        }
    }
}
