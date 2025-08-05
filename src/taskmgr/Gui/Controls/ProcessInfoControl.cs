using System.Diagnostics;
using System.Runtime.CompilerServices;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ProcessInfoControl : Control
{
    private readonly Theme _theme;
    private readonly ListView _processInfoView;
    private readonly ListView _menuView;
    private readonly ListView _modulesView;
    private readonly ListView _threadsView;
    private readonly ListView _handlesView;

    private readonly List<ListView> _tabControls = [];
    private ListView _focusedControl;
    
    private int _selectedProcessId = -1;

    private const int ControlGutter = 1;
    private const int ProcessInfoViewHeight = 8;
    private const int MenuViewWidth = 10;

    public ProcessInfoControl(ISystemTerminal terminal, Theme theme)
        : base(terminal)
    {
        _theme = theme;
        
        _processInfoView = new ListView(terminal) {
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

        _processInfoView.ColumnHeaders
            .Add(new ListViewColumnHeader("KEY"))
            .Add(new ListViewColumnHeader("VALUE"));

        _menuView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = true
        };

        _menuView.ColumnHeaders.Add(new ListViewColumnHeader("SELECT"));

        _threadsView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = true
        };

        _threadsView.ColumnHeaders
            .Add(new ListViewColumnHeader("THREAD ID"))
            .Add(new ListViewColumnHeader("STATE"))
            .Add(new ListViewColumnHeader("REASON"))
            .Add(new ListViewColumnHeader("PRI"));
        
        _modulesView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = false
        };

        _modulesView.ColumnHeaders
            .Add(new ListViewColumnHeader("MODULE"))
            .Add(new ListViewColumnHeader("PATH"));

        _handlesView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = false
        };

        _handlesView.ColumnHeaders
            .Add(new ListViewColumnHeader("ID"))
            .Add(new ListViewColumnHeader("NAME"));

        Controls
            .Add(_processInfoView)
            .Add(_menuView)
            .Add(_modulesView)
            .Add(_threadsView)
            .Add(_handlesView);
        
        _tabControls.AddRange(new [] {
            _modulesView, 
            _threadsView, 
            _handlesView
        });

        _focusedControl = _threadsView;
    }

    private void MenuViewOnItemClicked(object? sender, ListViewItemEventArgs e)
    {
        _tabControls.ForEach(ctrl => ctrl.Visible = false);
        
        var menuListViewItem = e.Item as MenuListViewItem;
        menuListViewItem!.AssociatedControl.Visible = true;
        
        Resize();
        Draw();
    }

    protected override void OnDraw()
    {
        try {
            Control.DrawingLockAcquire();

            if (_processInfoView.Items.Count == 0) {
                TryLoadProcessInfo();
            }
            
            _processInfoView.Draw();
            _menuView.Draw();

            Control activeControl = _tabControls.Single(ctrl => ctrl.Visible);
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
            ListView activeControl = _tabControls.Single(ctrl => ctrl.Visible);

            switch (keyInfo.Key) {
                case ConsoleKey.UpArrow:
                case ConsoleKey.DownArrow:
                case ConsoleKey.PageUp:
                case ConsoleKey.PageDown:
                    _focusedControl.KeyPressed(keyInfo, ref handled);
                    break;
                
                case ConsoleKey.LeftArrow:
                    _focusedControl = _menuView;
                    
                    if (activeControl.SelectedIndex <= _menuView.SelectedIndex) {
                        _menuView.SelectedIndex = activeControl.SelectedIndex;
                    }
                    else {
                        if (_menuView.Items.Count > 0) {
                            _menuView.SelectedIndex = _menuView.Items.Count - 1;
                        }
                    }
                    
                    _focusedControl.Draw();
                    ListViewItemEventArgs e = new(_menuView.SelectedItem);
                    MenuViewOnItemClicked(this, e);
                    break;
                
                case ConsoleKey.RightArrow:
                    _focusedControl = activeControl;
                    
                    if (_focusedControl.Items.Count > _menuView.Items.Count) {
                        _focusedControl.SelectedIndex = _menuView.SelectedIndex;
                    }
                    else {
                        if (_focusedControl.Items.Count > 0) {
                            _focusedControl.SelectedIndex = _focusedControl.Items.Count - 1;
                        }
                    }
                    
                    _focusedControl.Draw();
                    break;
            }
        }
        finally {
            Control.DrawingLockRelease();
        }
    }

    protected override void OnLoad()
    {
        _menuView.Items.Add(new MenuListViewItem(_threadsView, "THREADS"));
        _menuView.Items.Add(new MenuListViewItem(_modulesView, "MODULES"));
        _menuView.Items.Add(new MenuListViewItem(_handlesView, "HANDLES"));

        foreach (Control control in Controls) {
            control.Load();
        }
        
        _menuView.ItemClicked += MenuViewOnItemClicked;
    }

    protected override void OnResize()
    {
        Clear();
        
        _processInfoView.X = X;
        _processInfoView.Y = Y + 1;
        _processInfoView.Width = Width;
        _processInfoView.Height = ProcessInfoViewHeight;
        _processInfoView.ColumnHeaders[(int)InfoColumns.Key].Width = ColumnInfoKeyWidth;
        _processInfoView.ColumnHeaders[(int)InfoColumns.Value].Width = Width - ColumnInfoKeyWidth;
        _processInfoView.Resize();
        
        _menuView.X = X;
        _menuView.Y = _processInfoView.Y + _processInfoView.Height + ControlGutter;
        _menuView.Height = Height - (ProcessInfoViewHeight + 1 + ControlGutter);
        _menuView.Width = MenuViewWidth;
        _menuView.ColumnHeaders[0].Width = MenuViewWidth;
        _menuView.Resize();

        _tabControls.ForEach(ctrl => {
            ctrl.X = _menuView.X + _menuView.Width + ControlGutter;
            ctrl.Y = _menuView.Y;
            ctrl.Height = _menuView.Height;
            ctrl.Width = Width - (_menuView.Width + ControlGutter); 
        });
        
        _modulesView.ColumnHeaders[(int)ModuleColumns.ModuleName].Width = ColumnModuleNameWidth;
        _modulesView.ColumnHeaders[(int)ModuleColumns.FileName].Width = _modulesView.Width - ColumnModuleNameWidth;
        _modulesView.Resize();
        
        _threadsView.ColumnHeaders[(int)ThreadColumns.Id].Width = ColumnThreadIdWidth;
        _threadsView.ColumnHeaders[(int)ThreadColumns.State].Width = ColumnThreadStateWidth;
        _threadsView.ColumnHeaders[(int)ThreadColumns.Reason].Width = ColumnThreadReasonWidth;
        _threadsView.ColumnHeaders[(int)ThreadColumns.Priority].Width = ColumnThreadPriorityWidth;

        _threadsView.Resize();
        
        _handlesView.Resize();
    }

    protected override void OnUnload()
    {
        _processInfoView.Items.Clear();
        _menuView.Items.Clear();
        _modulesView.Items.Clear();
        _threadsView.Items.Clear();
        _handlesView.Items.Clear();
        
        foreach (Control control in Controls) {
            control.Unload();
        }
        
        _menuView.ItemClicked -= MenuViewOnItemClicked;
    }

    public int SelectedProcessId
    {
        get => _selectedProcessId;
        set => _selectedProcessId = value;
    }
    
    private void TryLoadProcessInfo()
    {
        try {
            if (!ProcessUtils.TryGetProcessByPid(SelectedProcessId, out Process? process)) {
                _processInfoView.Items.Clear();
                return;
            }

            _processInfoView.Items.Add(new(["Pid:", process!.Id.ToString()]) );
            _processInfoView.Items.Add(new(["File:", process!.MainModule!.ModuleName]) );
            _processInfoView.Items.Add(new(["Description:", ""]) );
            _processInfoView.Items.Add(new(["Path:", ProcessUtils.GetProcessCommandLine(process)]) );
            _processInfoView.Items.Add(new(["User:", ProcessUtils.GetProcessUserName(process)]) );
            _processInfoView.Items.Add(new(["Version:", ""]) );
            _processInfoView.Items.Add(new(["Size:", ""]) );
            _processInfoView.Items.Add(new(["", ""]) );

            var modules = ModuleInfo.GetModules(SelectedProcessId)
                .OrderBy(m => m.ModuleName)
                .ToList();

            foreach (var moduleInfo in modules) {
                _modulesView.Items.Add(new ModuleListViewItem(moduleInfo, _theme));
            }

            List<ThreadInfo> threads = ThreadInfo.GetThreads(SelectedProcessId)
                .OrderBy(m => m.ThreadId)
                .ToList();

            foreach (var threadInfo in threads) {
                _threadsView.Items.Add(new ThreadListViewItem(threadInfo, _theme));
            }
        }
        catch (Exception ex) {
            _processInfoView.Items.Add(new(new string[] { "Error:", ex.Message }));
        }
    }
}
