using Task.Manager.Configuration;
using Task.Manager.Extensions;
using Task.Manager.Process;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using IProcessor = Task.Manager.Process.IProcessor;
using ProcessorEventArgs = Task.Manager.Process.ProcessorEventArgs;

namespace Task.Manager.Gui.Controls;

public sealed partial class ProcessControl : Control
{
    private class CmdLineFilters
    {
        public int Pid { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string Process { get; init; } = string.Empty;
        public int NumProcs { get; init; }
    }
    
    private readonly IProcessor processor;
    private readonly AppConfig appConfig;
    private readonly ListView sortView;
    private readonly ListView processView;
    private readonly CmdLineFilters cmdLineFilters;

    private List<ProcessorInfo> allProcesses = [];
    private SystemStatistics systemStatistics;
    private ControlMode mode = ControlMode.None;
    private Columns sortColumn;

    private const int SortControlWidth = 20;
    private const int ControlGutter = 1;

    private const int InvalidSelectedItemIndex = -1;
    
    public event EventHandler<ListViewItemEventArgs>? ProcessItemSelected;
    
    public ProcessControl(
        IProcessor processor, 
        ISystemTerminal terminal, 
        AppConfig appConfig)
        : base(terminal)
    {
        this.processor = processor;
        this.appConfig = appConfig;
        
        cmdLineFilters = new CmdLineFilters {
            Pid = appConfig.FilterPid,
            UserName = appConfig.FilterUserName,
            Process = appConfig.FilterProcess,
            NumProcs = appConfig.NumberOfProcesses
        };

        Statistics sortStatistic = appConfig.SortColumn;
        
        sortColumn = sortStatistic switch {
            Statistics.Pid     => Columns.Pid,
            Statistics.Process => Columns.Process,
            Statistics.User    => Columns.User,
            Statistics.Pri     => Columns.Priority,
            Statistics.Cpu     => Columns.Cpu,
            Statistics.Mem     => Columns.Memory,
            Statistics.Thrd    => Columns.Threads,
            Statistics.Disk    => Columns.Disk,
            Statistics.Path    => Columns.CommandLine,
            _ => Columns.Cpu
        };
        
        sortView = new ListView(terminal) {
            Visible = mode == ControlMode.SortSelection,
            TabStop = true,
            TabIndex = 1
        };

        sortView.ColumnHeaders.Add(new ListViewColumnHeader("SORT BY"));

        processView = new ListView(terminal) {
            Visible = true,
            TabStop = true,
            TabIndex = 2
        };
        
        processView.ColumnHeaders
            .Add(new ListViewColumnHeader(Columns.Process.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Pid.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.User.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Priority.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Cpu.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Threads.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Memory.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.Disk.GetTitle()))
            .Add(new ListViewColumnHeader(Columns.CommandLine.GetTitle()));
        
        Controls
            .Add(sortView)
            .Add(processView);
    }

    public string FilterText { private get; set; } = string.Empty;

    private ListView? GetTargetControl()
    {
        ListView? targetControl = mode switch {
            ControlMode.None => processView,
            ControlMode.SortSelection => sortView,
            _ => null
        };

        return targetControl;
    }
    
    private void LoadSortItems()
    {
        IEnumerable<string> columns = Enum.GetValues<Columns>()
            .Where(c => c != Columns.Count)
            .Select(c => c.GetTitle());
        
        foreach (var column in columns) {
            sortView.Items.Add(new ListViewItem(column));
        }
    }
    
    protected override void OnDraw()
    {
        try {
            Control.DrawingLockAcquire();
            
            if (sortView.Items.Count == 0) {
                LoadSortItems();
            }
        
            UpdateListViewItems();
            
            sortView.Visible = mode == ControlMode.SortSelection;
            sortView.Draw();
            processView.Draw();
        }
        finally {
            Control.DrawingLockRelease();
        }
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        if (keyInfo.Key == ConsoleKey.Escape && mode == ControlMode.SortSelection) {
            SetMode(ControlMode.None);
            handled = true;
            return;
        }
        
        try {
            Control? targetControl = GetTargetControl();
            Control.DrawingLockAcquire();
            targetControl?.KeyPressed(keyInfo, ref handled);
        }
        finally {
            Control.DrawingLockRelease();
        }
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        BackgroundColour = appConfig.DefaultTheme.Background;
        ForegroundColour = appConfig.DefaultTheme.Foreground;
        
        ListView[] listViews = [sortView, processView];

        foreach (var listView in listViews) {
            listView.BackgroundHighlightColour = appConfig.DefaultTheme.BackgroundHighlight;
            listView.ForegroundHighlightColour = appConfig.DefaultTheme.ForegroundHighlight;
            listView.BackgroundColour = appConfig.DefaultTheme.Background;
            listView.ForegroundColour = appConfig.DefaultTheme.Foreground;
            listView.HeaderBackgroundColour = appConfig.DefaultTheme.HeaderBackground;
            listView.HeaderForegroundColour = appConfig.DefaultTheme.HeaderForeground;

            foreach (ListViewColumnHeader columnHeader in listView.ColumnHeaders) {
                columnHeader.BackgroundColour = appConfig.DefaultTheme.HeaderBackground;
                columnHeader.ForegroundColour = appConfig.DefaultTheme.HeaderForeground;
            }
        }
        
        processView.ColumnHeaders[(int)sortColumn].BackgroundColour = appConfig.DefaultTheme.BackgroundHighlight;
        processView.ColumnHeaders[(int)sortColumn].ForegroundColour = appConfig.DefaultTheme.ForegroundHighlight;
        processView.SetFocus();

        sortView.ItemSelected += SortViewOnItemSelected;
        processView.ItemSelected += ProcessViewOnItemSelected;
        processor.ProcessorUpdated += ProcessorOnProcessorUpdated;
    }

    protected override void OnResize()
    {
        sortView.X = X;
        sortView.Y = Y;
        sortView.Width = SortControlWidth;
        sortView.Height = Height;
        sortView.ColumnHeaders[0].Width = SortControlWidth;
        sortView.Resize();

        int pX = X;
        int pWidth = Width;
        
        if (mode == ControlMode.SortSelection) {
            pX = sortView.X + sortView.Width + ControlGutter;
            pWidth = Width - (sortView.Width + ControlGutter);
        }

        processView.X = pX;
        processView.Y = Y;
        processView.Width = pWidth;
        processView.Height = Height;
        
        processView.ColumnHeaders[(int)Columns.Process].Width = ColumnProcessWidth;
        processView.ColumnHeaders[(int)Columns.Pid].Width = ColumnPidWidth;
        processView.ColumnHeaders[(int)Columns.User].Width = ColumnUserWidth;
        processView.ColumnHeaders[(int)Columns.Priority].Width = ColumnPriorityWidth;
        processView.ColumnHeaders[(int)Columns.Priority].RightAligned = true;
        processView.ColumnHeaders[(int)Columns.Cpu].Width = ColumnCpuWidth;
        processView.ColumnHeaders[(int)Columns.Cpu].RightAligned = true;
        processView.ColumnHeaders[(int)Columns.Threads].Width = ColumnThreadsWidth;
        processView.ColumnHeaders[(int)Columns.Threads].RightAligned = true;
        processView.ColumnHeaders[(int)Columns.Memory].Width = ColumnMemoryWidth;
        processView.ColumnHeaders[(int)Columns.Memory].RightAligned = true;
        processView.ColumnHeaders[(int)Columns.Disk].Width = ColumnDiskWidth;
        processView.ColumnHeaders[(int)Columns.Disk].RightAligned = true;

        int total =
            ColumnProcessWidth +
            ColumnPidWidth +
            ColumnUserWidth +
            ColumnPriorityWidth +
            ColumnCpuWidth +
            ColumnThreadsWidth +
            ColumnMemoryWidth +
            ColumnDiskWidth;

        processView.ColumnHeaders[(int)Columns.CommandLine].Width = total + ColumnCommandlineWidth < processView.Width
            ? processView.ColumnHeaders[(int)Columns.CommandLine].Width = processView.Width - total
            : processView.ColumnHeaders[(int)Columns.CommandLine].Width = ColumnCommandlineWidth; 
        
        processView.Resize();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        
        sortView.ItemSelected -= SortViewOnItemSelected;
        processView.ItemSelected -= ProcessViewOnItemSelected;
        processor.ProcessorUpdated -= ProcessorOnProcessorUpdated;
    }

    private void ProcessorOnProcessorUpdated(object? sender, ProcessorEventArgs e)
    {
        allProcesses = e.ProcessInfos;
        systemStatistics = e.SystemStatistics;
        
        Draw();
    }

    private void ProcessViewOnItemSelected(object? sender, ListViewItemEventArgs e) =>
        ProcessItemSelected?.Invoke(sender, e);

    public int SelectedProcessId
    {
        get {
            if (processView.SelectedItem == null) {
                return InvalidSelectedItemIndex;
            }
            
            ListViewSubItem selectedSubItem = processView.SelectedItem.SubItems[(int)Columns.Pid];
            
            if (int.TryParse(selectedSubItem.Text, out int pid)) {
                return pid;
            }
            
            return InvalidSelectedItemIndex;
        }
    }

    public void SetMode(ControlMode mode)
    {
        if (mode == this.mode) {
            return;
        }

        this.mode = mode;
        sortView.Visible = this.mode == ControlMode.SortSelection;
        
        Control? targetControl = GetTargetControl();
        targetControl?.SetFocus();

        Clear();
        Resize();
        Draw();
    }
    
    private void SortViewOnItemSelected(object? sender, ListViewItemEventArgs e)
    {
        processView.ColumnHeaders[(int)sortColumn].BackgroundColour = appConfig.DefaultTheme.HeaderBackground;
        processView.ColumnHeaders[(int)sortColumn].ForegroundColour = appConfig.DefaultTheme.HeaderForeground;
        
        sortColumn = Enum.GetValues<Columns>().Single(c => c.GetTitle() == e.Item.Text);
        
        processView.ColumnHeaders[(int)sortColumn].BackgroundColour = appConfig.DefaultTheme.BackgroundHighlight;
        processView.ColumnHeaders[(int)sortColumn].ForegroundColour = appConfig.DefaultTheme.ForegroundHighlight;
        
        mode = ControlMode.None;
        processView.SetFocus();

        Clear();
        Resize();
        Draw();
    }

    private void UpdateListViewItems()
    {
        if (cmdLineFilters.Pid > -1) {
            allProcesses = allProcesses
                .Where(p => p.Pid == cmdLineFilters.Pid)
                .ToList();
        }
        else if (!string.IsNullOrWhiteSpace(cmdLineFilters.UserName)) {
            allProcesses = allProcesses
                .Where(p => p.UserName.Contains(cmdLineFilters.UserName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        else if (!string.IsNullOrWhiteSpace(cmdLineFilters.Process)) {
            allProcesses = allProcesses
                .Where(p => p.ProcessName.Contains(cmdLineFilters.Process, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(FilterText)) {
            allProcesses = allProcesses
                .Where(p => p.ProcessName.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase) ||
                            p.FileDescription.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase) ||
                            p.CmdLine.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase) ||
                            p.UserName.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        }

        // .DynamicOrderBy in Utils.QueryableExtensions is not supported for .net native compilation targets
        // hence the manual build out of the sorting below.

        List<ProcessorInfo> sortedProcesses = sortColumn switch {
            Columns.Cpu         => allProcesses.OrderByDescending(p => p.CpuTimePercent).ToList(),
            Columns.Disk        => allProcesses.OrderByDescending(p => p.DiskUsage).ToList(),
            Columns.Memory      => allProcesses.OrderByDescending(p => p.UsedMemory).ToList(),
            Columns.Pid         => allProcesses.OrderByDescending(p => p.Pid).ToList(),
            Columns.Priority    => allProcesses.OrderByDescending(p => p.BasePriority).ToList(),
            Columns.Process     => allProcesses.OrderByDescending(p => p.FileDescription).ToList(),
            Columns.Threads     => allProcesses.OrderByDescending(p => p.ThreadCount).ToList(),
            Columns.User        => allProcesses.OrderByDescending(p => p.UserName).ToList(),
            Columns.CommandLine => allProcesses.OrderByDescending(p => p.CmdLine).ToList(),
            _                   => []
        };

        if (cmdLineFilters.NumProcs > -1) {
            sortedProcesses = sortedProcesses
                .Take(cmdLineFilters.NumProcs)
                .ToList();
        }
        
        if (sortedProcesses.Count == 0) {
            processView.Items.Clear();
            return;
        }

        int selectedIndex = processView.SelectedIndex;
        
        for (int i = processView.Items.Count - 1; i >= 0; i--) {
            var item = (ProcessListViewItem)processView.Items[i];
            var exists = false;
            
            for (int j = 0; j < sortedProcesses.Count; j++) {
                if (sortedProcesses[j].Pid == item.Pid) {
                    exists = true;
                    break;
                }
            }
        
            if (!exists) {
                processView.Items.RemoveAt(i);
            }
        }

        for (int i = 0; i < sortedProcesses.Count; i++) {
            var exists = false;
            
            for (int j = 0; j < processView.Items.Count; j++) {
                var item = (ProcessListViewItem)processView.Items[j];
                
                if (sortedProcesses[i].Pid == item.Pid) {
                    item.UpdateItem(sortedProcesses[i], ref systemStatistics);
        
                    int insertAt = i > processView.Items.Count - 1 
                        ? processView.Items.Count - 1 
                        : i;
        
                    processView.Items.RemoveAt(j);
                    processView.Items.InsertAt(insertAt, item);
                    
                    exists = true;
                    break;
                }
            }
        
            if (!exists) {
                ProcessListViewItem item = new(sortedProcesses[i], ref systemStatistics, appConfig);
                processView.Items.InsertAt(i, item);
            }
        }

        if (processView.Items.Count > 0) {
            processView.SelectedIndex = selectedIndex >= 0 && selectedIndex < processView.Items.Count
                ? selectedIndex
                : 0;
        }
    }
}
