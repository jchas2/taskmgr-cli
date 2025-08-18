using System.Numerics;
using Task.Manager.Configuration;
using Task.Manager.Extensions;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public sealed partial class ProcessControl : Control
{
    private readonly IProcessor processor;
    private readonly Theme theme;
    private readonly Config config;
    private readonly ListView sortView;
    private readonly ListView processView;

    private List<ProcessInfo> allProcesses = [];
    private SystemStatistics systemStatistics;
    private ControlMode mode = ControlMode.None;
    private Columns sortColumn = Columns.Cpu;

    private const int SortControlWidth = 20;
    private const int ControlGutter = 1;

    private const int InvalidSelectedItemIndex = -1;
    
    public ProcessControl(
        IProcessor processor, 
        ISystemTerminal terminal, 
        Theme theme,
        Config config)
        : base(terminal)
    {
        this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
        this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
        this.config = config ?? throw new ArgumentNullException(nameof(config));

        sortView = new ListView(terminal) {
            BackgroundHighlightColour = theme.BackgroundHighlight,
            ForegroundHighlightColour = theme.ForegroundHighlight,
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            HeaderBackgroundColour = theme.HeaderBackground,
            HeaderForegroundColour = theme.HeaderForeground,
            Visible = mode == ControlMode.SortSelection
        };

        sortView.ColumnHeaders.Add(new ListViewColumnHeader("SORT BY"));

        processView = new ListView(terminal) {
            BackgroundHighlightColour = this.theme.BackgroundHighlight,
            ForegroundHighlightColour = this.theme.ForegroundHighlight,
            BackgroundColour = this.theme.Background,
            ForegroundColour = this.theme.Foreground,
            HeaderBackgroundColour = this.theme.HeaderBackground,
            HeaderForegroundColour = this.theme.HeaderForeground,
            Visible = true
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
        
        processView.ColumnHeaders[(int)sortColumn].BackgroundColour = this.theme.BackgroundHighlight;
        processView.ColumnHeaders[(int)sortColumn].ForegroundColour = this.theme.ForegroundHighlight;
        
        Controls
            .Add(sortView)
            .Add(processView);
    }

    public string FilterText { private get; set; } = string.Empty;
    
    private void LoadSortItems()
    {
        IEnumerable<string> columns = Enum.GetValues<Columns>()
            .Where(c => c != Columns.Count)
            .Select(c => c.GetTitle());
        
        foreach (var column in columns) {
            sortView.Items.Add(new ListViewItem(
                column, 
                theme.Background, 
                theme.Foreground));
        }
    }

    protected override void OnDraw()
    {
        try {
            Control.DrawingLockAcquire();
            
            if (sortView.Items.Count == 0) {
                LoadSortItems();
            }
        
            UpdateListViewItems(allProcesses, ref systemStatistics);
        
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
        try {
            Control.DrawingLockAcquire();

            Control? targetControl = mode switch {
                ControlMode.None => processView,
                ControlMode.SortSelection => sortView,
                _ => null
            };

            targetControl?.KeyPressed(keyInfo, ref handled);
        }
        finally {
            Control.DrawingLockRelease();
        }
    }

    protected override void OnLoad()
    {
        sortView.Load();
        processView.Load();
        
        sortView.ItemSelected += SortViewOnItemSelected;
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
        sortView.Unload();
        processView.Unload();
        
        sortView.ItemSelected -= SortViewOnItemSelected;
        processor.ProcessorUpdated -= ProcessorOnProcessorUpdated;
    }
    
    private void ProcessorOnProcessorUpdated(object? sender, ProcessorEventArgs e)
    {
        allProcesses = e.ProcessInfos;
        systemStatistics = e.SystemStatistics;
        
        Draw();
    }
    
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

        Clear();
        Resize();
        Draw();
    }
    
    private void SortViewOnItemSelected(object? sender, ListViewItemEventArgs e)
    {
        processView.ColumnHeaders[(int)sortColumn].BackgroundColour = theme.HeaderBackground;
        processView.ColumnHeaders[(int)sortColumn].ForegroundColour = theme.HeaderForeground;
        
        sortColumn = Enum.GetValues<Columns>().Single(c => c.GetTitle() == e.Item.Text);
        
        processView.ColumnHeaders[(int)sortColumn].BackgroundColour = theme.BackgroundHighlight;
        processView.ColumnHeaders[(int)sortColumn].ForegroundColour = theme.ForegroundHighlight;
        
        mode = ControlMode.None;
        
        Resize();
        Draw();
    }

    private void UpdateListViewItems(List<ProcessInfo> allProcesses, ref SystemStatistics systemStatistics)
    {
        // .DynamicOrderBy in Utils.QueryableExtensions is not supported for .net native compilation targets
        // hence the manual build out of the query below.
        if (FilterText != string.Empty) {
            allProcesses = allProcesses
                .Where(p => p.ExeName.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase) ||
                            p.FileDescription.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase) ||
                            p.CmdLine.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase) ||
                            p.UserName.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        }

        List<ProcessInfo> sortedProcesses = sortColumn switch {
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
                ProcessListViewItem item = new(sortedProcesses[i], ref systemStatistics, theme);
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
