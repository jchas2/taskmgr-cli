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
        
        _listView = new ListView(terminal);
        _listView.BackgroundHighlightColour = _theme.BackgroundHighlight;
        _listView.ForegroundHighlightColour = _theme.ForegroundHighlight;
        _listView.BackgroundColour = _theme.Background;
        _listView.ForegroundColour = _theme.Foreground;
        _listView.HeaderBackgroundColour = _theme.HeaderBackground;
        _listView.HeaderForegroundColour = _theme.HeaderForeground;
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("THREAD ID"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("STATE"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("REASON"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("PRI"));
        
        Controls.Add(_listView);
        
        Theme = theme;
    }
    
    private void LoadThreadInfos()
    {
        _listView.Items.Clear();

        var threads = ThreadInfo.GetThreads(SelectedProcessId)
            .OrderBy(m => m.ThreadId)
            .ToList();
        
        foreach (var threadInfo in threads) {
            var item = new ThreadListViewItem(threadInfo, Theme);
            _listView.Items.Add(item);
        }
    }
    
    protected override void OnDraw()
    {
        UpdateColumnHeaders();
        LoadThreadInfos();
        
        _listView.Draw();
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo)
    {
        _listView.KeyPressed(keyInfo);
    }

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
    }

    public int SelectedProcessId
    {
        get => _selectedProcessId;
        set => _selectedProcessId = value;
    }

    private Theme Theme { get; }

    private void UpdateColumnHeaders()
    {
        _listView.ColumnHeaders[(int)Columns.Id].Width = ColumnIdWidth;
        _listView.ColumnHeaders[(int)Columns.State].Width = ColumnStateWidth;
        _listView.ColumnHeaders[(int)Columns.Reason].Width = ColumnReasonWidth;
        _listView.ColumnHeaders[(int)Columns.Priority].Width = ColumnPriorityWidth;

         for (int i = 0; i < (int)Columns.Count; i++) {
             _listView.ColumnHeaders[i].BackgroundColour = Theme.HeaderBackground;
             _listView.ColumnHeaders[i].ForegroundColour = Theme.HeaderForeground;
         }
    }
}