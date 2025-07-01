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
    private readonly ListView _processView;

    private ProcessInfo[] _allProcesses = [];
    private Lock _lock = new();
    private ControlMode _mode = ControlMode.None;

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
        _sortControl.Visible = _mode == ControlMode.SortSelection;
        
        _processView = new ListView(terminal);
        _processView.BackgroundHighlightColour = _theme.BackgroundHighlight;
        _processView.ForegroundHighlightColour = _theme.ForegroundHighlight;
        _processView.BackgroundColour = _theme.Background;
        _processView.ForegroundColour = _theme.Foreground;
        _processView.HeaderBackgroundColour = _theme.HeaderBackground;
        _processView.HeaderForegroundColour = _theme.HeaderForeground;
        _processView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Process.GetDescription()));
        _processView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Pid.GetDescription()));
        _processView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.User.GetDescription()));
        _processView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Priority.GetDescription()));
        _processView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Cpu.GetDescription()));
        _processView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Threads.GetDescription()));
        _processView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.Memory.GetDescription()));
        _processView.ColumnHeaders.Add(new ListViewColumnHeader(Columns.CommandLine.GetDescription()));
        _processView.Visible = true;
        
        Controls.Add(_sortControl);
        Controls.Add(_processView);
        
        Theme = theme;
    }

    protected override void OnDraw()
    {
        lock (_lock) {
            UpdateListViewItems(_allProcesses);
            
            _sortControl.Draw();
            _processView.Draw();
        }
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo)
    {
        lock (_lock) {
            Control? targetControl = _mode switch {
                ControlMode.None => _processView,
                ControlMode.SortSelection => _sortControl,
                _ => null
            };
            
            targetControl?.KeyPressed(keyInfo);
        }
    }

    protected override void OnLoad()
    {
        _sortControl.Load();
        _processView.Load();
        
        _processor.ProcessorUpdated += ProcessorOnProcessorUpdated;
    }

    protected override void OnResize()
    {
        _sortControl.X = X;
        _sortControl.Y = Y;
        _sortControl.Width = SortControlWidth;
        _sortControl.Height = Height;
        _sortControl.Resize();

        int pX = X;
        int pWidth = Width;
        
        if (_mode == ControlMode.SortSelection) {
            pX = _sortControl.X + _sortControl.Width + 2;
            pWidth = Width - (_sortControl.Width + 2);
        }

        _processView.X = pX;
        _processView.Y = Y;
        _processView.Width = pWidth;
        _processView.Height = Height;
        
#if __APPLE__
        // Bug on MacOS where ProcessName returns truncated 15 char value.
        _processView.ColumnHeaders[(int)Columns.Process].Width = 16;
#elif __WIN32__ 
        _listView.ColumnHeaders[(int)Columns.Process].Width = ColumnProcessWidth;
#endif
        _processView.ColumnHeaders[(int)Columns.Pid].Width = ColumnPidWidth;
        _processView.ColumnHeaders[(int)Columns.User].Width = ColumnUserWidth;
        _processView.ColumnHeaders[(int)Columns.Priority].Width = ColumnPriorityWidth;
        _processView.ColumnHeaders[(int)Columns.Priority].RightAligned = true;
        _processView.ColumnHeaders[(int)Columns.Cpu].Width = ColumnCpuWidth;
        _processView.ColumnHeaders[(int)Columns.Cpu].RightAligned = true;
        _processView.ColumnHeaders[(int)Columns.Threads].Width = ColumnThreadsWidth;
        _processView.ColumnHeaders[(int)Columns.Threads].RightAligned = true;
        _processView.ColumnHeaders[(int)Columns.Memory].Width = ColumnMemoryWidth;
        _processView.ColumnHeaders[(int)Columns.Memory].RightAligned = true;

        int total =
            ColumnProcessWidth + ColumnMargin +
            ColumnPidWidth + ColumnMargin +
            ColumnUserWidth + ColumnMargin +
            ColumnPriorityWidth + ColumnMargin +
            ColumnCpuWidth + ColumnMargin +
            ColumnThreadsWidth + ColumnMargin +
            ColumnMemoryWidth + ColumnMargin;

        if (total + ColumnCommandlineWidth + ColumnMargin < _processView.Width) {
            _processView.ColumnHeaders[(int)Columns.CommandLine].Width = _processView.Width - total - ColumnMargin;    
        }
        else {
            _processView.ColumnHeaders[(int)Columns.CommandLine].Width = ColumnCommandlineWidth;
        }
        
        _processView.Resize();
    }

    protected override void OnUnload()
    {
        _sortControl.Unload();
        _processView.Unload();
        
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
            var selectedSubItem = _processView.SelectedItem.SubItems[(int)Columns.Pid];
            
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
        
        if (_processView.Items.Count == 0) {
            
            for (int i = 0; i < sortedProcesses.Length; i++) {
                var item = new ProcessListViewItem(ref sortedProcesses[i], Theme);
                _processView.Items.Add(item);
            }
            
            return;
        }

        for (int i = 0; i < sortedProcesses.Length; i++) {
            bool found = false;
            
            for (int j = 0; j < _processView.Items.Count; j++) {
                var item = (ProcessListViewItem)_processView.Items[j];
                
                if (sortedProcesses[i].Pid == item.Pid) {
                    item.UpdateItem(ref sortedProcesses[i]);

                    int insertAt = i > _processView.Items.Count - 1 
                        ? _processView.Items.Count - 1 
                        : i;

                    _processView.Items.InsertAt(insertAt, item);
                    _processView.Items.RemoveAt(j);
                    
                    found = true;
                    break;
                }
            }

            if (false == found) {
                var item = new ProcessListViewItem(ref sortedProcesses[i], Theme);
                _processView.Items.InsertAt(i, item);
            }
        }
    }
}
