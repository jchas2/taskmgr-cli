using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ThreadsControl : Control
{
    private readonly Theme _theme;
    private readonly ListView _listView;

    private int _selectedProcessId = -1;
    
    public ThreadsControl(ISystemTerminal terminal, Theme theme)
        : base(terminal)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));

        _listView = new ListView(terminal) {
            BackgroundHighlightColour = _theme.BackgroundHighlight,
            ForegroundHighlightColour = _theme.ForegroundHighlight,
            BackgroundColour = _theme.Background,
            ForegroundColour = _theme.Foreground,
            HeaderBackgroundColour = _theme.HeaderBackground,
            HeaderForegroundColour = _theme.HeaderForeground
        };                                                  
        
        _listView.ColumnHeaders
            .Add(new ListViewColumnHeader("THREAD ID"))
            .Add(new ListViewColumnHeader("STATE"))
            .Add(new ListViewColumnHeader("REASON"))
            .Add(new ListViewColumnHeader("PRI"));
        
        Controls.Add(_listView);
    }
    
    private void LoadThreadInfos()
    {
        _listView.Items.Clear();

        List<ThreadInfo> threads = ThreadInfo.GetThreads(SelectedProcessId)
            .OrderBy(m => m.ThreadId)
            .ToList();
        
        foreach (var threadInfo in threads) {
            ThreadListViewItem item = new(threadInfo, _theme);
            _listView.Items.Add(item);
        }
    }
    
    protected override void OnDraw()
    {
        LoadThreadInfos();
        
        _listView.Draw();
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled) =>
        _listView.KeyPressed(keyInfo, ref handled);

    protected override void OnLoad()
    {
        _listView.Load();
    }

    protected override void OnResize()
    {
        _listView.X = X;
        _listView.Y = Y;
        _listView.Width = Width;
        _listView.Height = Height;
        
        _listView.ColumnHeaders[(int)Columns.Id].Width = ColumnIdWidth;
        _listView.ColumnHeaders[(int)Columns.State].Width = ColumnStateWidth;
        _listView.ColumnHeaders[(int)Columns.Reason].Width = ColumnReasonWidth;
        _listView.ColumnHeaders[(int)Columns.Priority].Width = ColumnPriorityWidth;

        for (int i = 0; i < (int)Columns.Count; i++) {
            _listView.ColumnHeaders[i].BackgroundColour = _theme.HeaderBackground;
            _listView.ColumnHeaders[i].ForegroundColour = _theme.HeaderForeground;
        }
        
        _listView.Resize();
    }

    public int SelectedProcessId
    {
        get => _selectedProcessId;
        set => _selectedProcessId = value;
    }
}