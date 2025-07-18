using System.Diagnostics;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.Extensions;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public sealed partial class ProcessControl : Control
{
    private readonly IProcessor _processor;
    private readonly Theme _theme;
    private readonly ListView _sortView;
    private readonly ListView _processView;

    private ProcessInfo[] _allProcesses = [];
    private SystemStatistics _systemStatistics;
    private Lock _lock = new();
    private ControlMode _mode = ControlMode.None;
    private Columns _sortColumn = Columns.Cpu;

    private const int SortControlWidth = 20;
    
    public ProcessControl(
        IProcessor processor, 
        ISystemTerminal terminal, 
        Theme theme)
        : base(terminal)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));

        _sortView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = _mode == ControlMode.SortSelection
        };

        _sortView.ColumnHeaders.Add(new ListViewColumnHeader("SORT BY"));

        _processView = new ListView(terminal) {
            BackgroundHighlightColour = _theme.BackgroundHighlight,
            ForegroundHighlightColour = _theme.ForegroundHighlight,
            BackgroundColour = _theme.Background,
            ForegroundColour = _theme.Foreground,
            HeaderBackgroundColour = _theme.HeaderBackground,
            HeaderForegroundColour = _theme.HeaderForeground,
            Visible = true
        };
        
        _processView.ColumnHeaders
            .Add(new ListViewColumnHeader(Columns.Process.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Pid.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.User.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Priority.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Cpu.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Threads.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Memory.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.CommandLine.GetTitle()));
        
        _processView.ColumnHeaders[(int)_sortColumn].BackgroundColour = _theme.BackgroundHighlight;
        _processView.ColumnHeaders[(int)_sortColumn].ForegroundColour = _theme.ForegroundHighlight;
        
        Controls
            .Add(_sortView)
            .Add(_processView);
    }

    private void LoadSortItems()
    {
        IEnumerable<string> columns = Enum.GetValues<Columns>()
            .Where(c => c != Columns.Count)
            .Select(c => c.GetTitle());
        
        foreach (var column in columns) {
            _sortView.Items.Add(new ListViewItem(column));
        }
    }
    
    protected override void OnDraw()
    {
        lock (_lock) {
            if (_sortView.Items.Count == 0) {
                LoadSortItems();
            }
            
            UpdateListViewItems(_allProcesses, ref _systemStatistics);
            
            _sortView.Visible = _mode == ControlMode.SortSelection;
            _sortView.Draw();
            _processView.Draw();
        }
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        lock (_lock) {
            Control? targetControl = _mode switch {
                ControlMode.None => _processView,
                ControlMode.SortSelection => _sortView,
                _ => null
            };
            
            targetControl?.KeyPressed(keyInfo, ref handled);
        }
    }

    protected override void OnLoad()
    {
        _sortView.Load();
        _processView.Load();
        
        _sortView.ItemClicked += SortViewOnItemClicked;
        _processor.ProcessorUpdated += ProcessorOnProcessorUpdated;
    }
    
    protected override void OnResize()
    {
        _sortView.X = X;
        _sortView.Y = Y;
        _sortView.Width = SortControlWidth;
        _sortView.Height = Height;
        _sortView.ColumnHeaders[0].Width = SortControlWidth;
        _sortView.Resize();

        int pX = X;
        int pWidth = Width;
        
        if (_mode == ControlMode.SortSelection) {
            pX = _sortView.X + _sortView.Width + 2;
            pWidth = Width - (_sortView.Width + 2);
        }

        _processView.X = pX;
        _processView.Y = Y;
        _processView.Width = pWidth;
        _processView.Height = Height;
        
        _processView.ColumnHeaders[(int)Columns.Process].Width = ColumnProcessWidth;
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
            ColumnProcessWidth +
            ColumnPidWidth + 
            ColumnUserWidth + 
            ColumnPriorityWidth + 
            ColumnCpuWidth + 
            ColumnThreadsWidth +
            ColumnMemoryWidth;

        _processView.ColumnHeaders[(int)Columns.CommandLine].Width = total + ColumnCommandlineWidth < _processView.Width
            ? _processView.ColumnHeaders[(int)Columns.CommandLine].Width = _processView.Width - total
            : _processView.ColumnHeaders[(int)Columns.CommandLine].Width = ColumnCommandlineWidth; 
        
        _processView.Resize();
    }

    protected override void OnUnload()
    {
        _sortView.Unload();
        _processView.Unload();
        
        _sortView.ItemClicked -= SortViewOnItemClicked;
        _processor.ProcessorUpdated -= ProcessorOnProcessorUpdated;
    }
    
    private void ProcessorOnProcessorUpdated(object? sender, ProcessorEventArgs e)
    {
        _allProcesses = e.ProcessInfos;
        _systemStatistics = e.SystemStatistics;
        
        Draw();
    }
    
    public int SelectedProcessId
    {
        get {
            ListViewSubItem selectedSubItem = _processView.SelectedItem.SubItems[(int)Columns.Pid];
            
            if (int.TryParse(selectedSubItem.Text, out int pid)) {
                return pid;
            }
            
            return -1;
        }
    }

    public void SetMode(ControlMode mode)
    {
        if (mode == _mode) {
            return;
        }

        _mode = mode;
        _sortView.Visible = _mode == ControlMode.SortSelection;

        Clear();
        Resize();
        Draw();
    }
    
    private void SortViewOnItemClicked(object? sender, ListViewItemEventArgs e)
    {
        _processView.ColumnHeaders[(int)_sortColumn].BackgroundColour = _theme.HeaderBackground;
        _processView.ColumnHeaders[(int)_sortColumn].ForegroundColour = _theme.HeaderForeground;
        
        _sortColumn = Enum.GetValues<Columns>().Single(c => c.GetTitle() == e.Item.Text);
        
        _processView.ColumnHeaders[(int)_sortColumn].BackgroundColour = _theme.BackgroundHighlight;
        _processView.ColumnHeaders[(int)_sortColumn].ForegroundColour = _theme.ForegroundHighlight;
        
        _mode = ControlMode.None;
        
        Resize();
        Draw();
    }

    private void UpdateListViewItems(ProcessInfo[] allProcesses, ref SystemStatistics systemStatistics)
    {
        var sortedProcesses = allProcesses.AsQueryable()
            .DynamicOrderBy(_sortColumn.GetProperty(), isDescending: true)
            .ToArray();
        
        if (_processView.Items.Count == 0) {
            
            for (int i = 0; i < sortedProcesses.Length; i++) {
                ProcessListViewItem item = new(
                    ref sortedProcesses[i],
                    ref systemStatistics,
                    _theme);
                
                _processView.Items.Add(item);
            }
            
            return;
        }

        for (int i = 0; i < sortedProcesses.Length; i++) {
            var found = false;
            
            for (int j = 0; j < _processView.Items.Count; j++) {
                var item = (ProcessListViewItem)_processView.Items[j];
                
                if (sortedProcesses[i].Pid == item.Pid) {
                    item.UpdateItem(ref sortedProcesses[i], ref systemStatistics);

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
                ProcessListViewItem item = new(
                    ref sortedProcesses[i],
                    ref systemStatistics,
                    _theme);
                
                _processView.Items.InsertAt(i, item);
            }
        }
    }
}
