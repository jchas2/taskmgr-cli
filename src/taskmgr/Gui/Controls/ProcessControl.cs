using System.Diagnostics;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public sealed partial class ProcessControl : Control
{
    private readonly IProcessor _processor;
    private readonly Theme _theme;
    private readonly ListView _listView;

    private ProcessInfo[] _allProcesses = [];
    private Lock _lock = new();
    
    private int _cachedWidth = 0;
    
    public ProcessControl(
        IProcessor processor, 
        ISystemTerminal terminal, 
        Theme theme)
        : base(terminal)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        
        _listView = new ListView(terminal);
        _listView.BackgroundHighlightColour = _theme.BackgroundHighlight;
        _listView.ForegroundHighlightColour = _theme.ForegroundHighlight;
        _listView.BackgroundColour = _theme.Background;
        _listView.ForegroundColour = _theme.Foreground;
        _listView.HeaderBackgroundColour = _theme.HeaderBackground;
        _listView.HeaderForegroundColour = _theme.HeaderForeground;
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("PROCESS"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("PID"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("USER"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("PRI"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("CPU%"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("THRDS"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("MEM"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("PATH"));
        
        Controls.Add(_listView);
        
        Theme = theme;
    }

    protected override void OnDraw()
    {
        lock (_lock) {
            _listView.X = X;
            _listView.Y = Y;
            _listView.Width = Width;
            _listView.Height = Height;

            UpdateColumnHeaders();
            UpdateListViewItems(_allProcesses);

            _listView.Draw();
        }
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo)
    {
        lock (_lock) {
            _listView.KeyPressed(keyInfo);
        }
    }

    protected override void OnLoad()
    {
        _listView.X = X;
        _listView.Y = Y;
        _listView.Width = Width;
        _listView.Load();
        
        _processor.ProcessorUpdated += ProcessorOnProcessorUpdated;
    }

    protected override void OnUnload()
    {
        _listView.Unload();
        
        _processor.ProcessorUpdated -= ProcessorOnProcessorUpdated;
    }
    
    private void ProcessorOnProcessorUpdated(object? sender, ProcessorEventArgs e)
    {
        _allProcesses = e.ProcessInfos;
        
        Draw();
    }
    
    public int SelectedProcessId
    {
        get {
            var selectedSubItem = _listView.SelectedItem.SubItems[(int)Columns.Pid];
            
            if (int.TryParse(selectedSubItem.Text, out int pid)) {
                return pid;
            }
            
            return -1;
        }
    }

    private Theme Theme { get; }

    private void UpdateColumnHeaders()
    {
        if (_cachedWidth == Width) {
            return;
        }
#if __APPLE__
        // Bug on MacOS where ProcessName returns truncated 15 char value.
        _listView.ColumnHeaders[(int)Columns.Process].Width = 16;
#elif __WIN32__ 
        _listView.ColumnHeaders[(int)Columns.Process].Width = ColumnProcessWidth;
#endif
        _listView.ColumnHeaders[(int)Columns.Pid].Width = ColumnPidWidth;
        _listView.ColumnHeaders[(int)Columns.User].Width = ColumnUserWidth;
        _listView.ColumnHeaders[(int)Columns.Priority].Width = ColumnPriorityWidth;
        _listView.ColumnHeaders[(int)Columns.Priority].RightAligned = true;
        _listView.ColumnHeaders[(int)Columns.Cpu].Width = ColumnCpuWidth;
        _listView.ColumnHeaders[(int)Columns.Cpu].RightAligned = true;
        _listView.ColumnHeaders[(int)Columns.Threads].Width = ColumnThreadsWidth;
        _listView.ColumnHeaders[(int)Columns.Threads].RightAligned = true;
        _listView.ColumnHeaders[(int)Columns.Memory].Width = ColumnMemoryWidth;
        _listView.ColumnHeaders[(int)Columns.Memory].RightAligned = true;

        int total =
            ColumnProcessWidth + ColumnMargin +
            ColumnPidWidth + ColumnMargin +
            ColumnUserWidth + ColumnMargin +
            ColumnPriorityWidth + ColumnMargin +
            ColumnCpuWidth + ColumnMargin +
            ColumnThreadsWidth + ColumnMargin +
            ColumnMemoryWidth + ColumnMargin;

        if (total + ColumnCommandlineWidth + ColumnMargin < Width) {
            _listView.ColumnHeaders[(int)Columns.CommandLine].Width = Width - total - ColumnMargin;    
        }
        else {
            _listView.ColumnHeaders[(int)Columns.CommandLine].Width = ColumnCommandlineWidth;
        }

        for (int i = 0; i < (int)Columns.Count; i++) {
            _listView.ColumnHeaders[i].BackgroundColour = Theme.HeaderBackground;
            _listView.ColumnHeaders[i].ForegroundColour = Theme.HeaderForeground;
        }
        
        _cachedWidth = Width;
    }
    
    private void UpdateListViewItems(ProcessInfo[] allProcesses)
    {
        var sortedProcesses = allProcesses
            .OrderByDescending(p => p.CpuTimePercent)
            .ToArray();
        
        if (_listView.Items.Count == 0) {
            
            for (int i = 0; i < sortedProcesses.Length; i++) {
                var item = new ProcessListViewItem(ref sortedProcesses[i], Theme);
                _listView.Items.Add(item);
            }
            
            return;
        }

        for (int i = 0; i < sortedProcesses.Length; i++) {
            bool found = false;
            
            for (int j = 0; j < _listView.Items.Count; j++) {
                var item = (ProcessListViewItem)_listView.Items[j];
                
                if (sortedProcesses[i].Pid == item.Pid) {
                    item.UpdateItem(ref sortedProcesses[i]);

                    int insertAt = i > _listView.Items.Count - 1 
                        ? _listView.Items.Count - 1 
                        : i;

                    _listView.Items.InsertAt(insertAt, item);
                    _listView.Items.RemoveAt(j);
                    
                    found = true;
                    break;
                }
            }

            if (false == found) {
                var item = new ProcessListViewItem(ref sortedProcesses[i], Theme);
                _listView.Items.InsertAt(i, item);
            }
        }
    }
}
