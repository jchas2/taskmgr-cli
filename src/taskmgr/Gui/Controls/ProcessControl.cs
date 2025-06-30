using System.Diagnostics;
using Task.Manager.Cli.Utils;
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
    private readonly ProcessSortControl _sortControl;
    private readonly ListView _listView;

    private ProcessInfo[] _allProcesses = [];
    private Lock _lock = new();

    private const int SortControlWidth = 20;
    
    public ProcessControl(
        IProcessor processor, 
        ISystemTerminal terminal, 
        Theme theme)
        : base(terminal)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        
        _sortControl = new ProcessSortControl(terminal, theme);
        
        _listView = new ListView(terminal);
        _listView.BackgroundHighlightColour = _theme.BackgroundHighlight;
        _listView.ForegroundHighlightColour = _theme.ForegroundHighlight;
        _listView.BackgroundColour = _theme.Background;
        _listView.ForegroundColour = _theme.Foreground;
        _listView.HeaderBackgroundColour = _theme.HeaderBackground;
        _listView.HeaderForegroundColour = _theme.HeaderForeground;
        _listView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Process.GetDescription()));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Pid.GetDescription()));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.User.GetDescription()));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Priority.GetDescription()));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Cpu.GetDescription()));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Threads.GetDescription()));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Memory.GetDescription()));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.CommandLine.GetDescription()));
        
        Controls.Add(_sortControl);
        Controls.Add(_listView);
        
        Theme = theme;
    }

    protected override void OnDraw()
    {
        lock (_lock) {
            UpdateListViewItems(_allProcesses);
            
            _sortControl.Draw();
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
        _sortControl.Load();
        _listView.Load();
        
        _processor.ProcessorUpdated += ProcessorOnProcessorUpdated;
    }

    protected override void OnResize()
    {
        _sortControl.X = X;
        _sortControl.Y = Y;
        _sortControl.Width = SortControlWidth;
        _sortControl.Height = Height;
        _sortControl.Resize();
        
        _listView.X = _sortControl.X + _sortControl.Width + 2;
        _listView.Y = Y;
        _listView.Width = Width - (_sortControl.Width + 2);
        _listView.Height = Height;
        
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

        if (total + ColumnCommandlineWidth + ColumnMargin < _listView.Width) {
            _listView.ColumnHeaders[(int)Columns.CommandLine].Width = _listView.Width - total - ColumnMargin;    
        }
        else {
            _listView.ColumnHeaders[(int)Columns.CommandLine].Width = ColumnCommandlineWidth;
        }
        
        _listView.Resize();
    }

    protected override void OnUnload()
    {
        _sortControl.Unload();
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
