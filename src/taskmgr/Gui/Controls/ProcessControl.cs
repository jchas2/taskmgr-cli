using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public sealed partial class ProcessControl : Control
{
    private ProcessInfo[] _allProcesses;
    private readonly object _processLock = new();
    private readonly IProcesses _processes;
    private readonly ISystemInfo _systemInfo;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly SystemStatistics _systemStatistics;

    private readonly HeaderControl _headerControl;
    private readonly ListView _listView;

    private int _cachedTerminalWidth = 0;
    
    public ProcessControl(
        ISystemTerminal terminal, 
        IProcesses processes, 
        ISystemInfo systemInfo,
        Theme theme)
        : base(terminal)
    {
        _allProcesses = [];
        _processes = processes ?? throw new ArgumentNullException(nameof(processes));
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
        _systemStatistics = new SystemStatistics();
        
        _headerControl = new HeaderControl(Terminal);
        _headerControl.BackgroundColour = theme.Background;
        _headerControl.ForegroundColour = theme.Foreground;
        _headerControl.MenubarColour = theme.Menubar;
        
        _listView = new ListView(Terminal);
        _listView.BackgroundHighlightColour = theme.BackgroundHighlight;
        _listView.ForegroundHighlightColour = theme.ForegroundHighlight;
        _listView.BackgroundColour = theme.Background;
        _listView.ForegroundColour = theme.Foreground;
        _listView.HeaderBackgroundColour = theme.HeaderBackground;
        _listView.HeaderForegroundColour = theme.HeaderForeground;
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("PROCESS"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("PID"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("USER"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("PRI"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("CPU%"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("THRDS"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("MEM"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("PATH"));
        
        Controls.Add(_headerControl);
        Controls.Add(_listView);
        
        Theme = theme;
    }

    private void Draw()
    {
        var bounds = new Rectangle(
            x: 0, 
            y: 0, 
            Terminal.WindowWidth, 
            Terminal.WindowHeight);
        
        _headerControl.Draw(_systemStatistics, ref bounds);

        _listView.Draw(bounds);
    }
    
    private void GetTotalSystemTimes()
    {
        _systemStatistics.CpuPercentIdleTime = 0.0;
        _systemStatistics.CpuPercentUserTime = 0.0;
        _systemStatistics.CpuPercentKernelTime = 0.0;
        
        lock (_processLock) {
            for (int i = 0; i < _allProcesses.Length; i++) {
                _systemStatistics.CpuPercentUserTime += _allProcesses[i].CpuUserTimePercent;
                _systemStatistics.CpuPercentKernelTime += _allProcesses[i].CpuKernelTimePercent;
            }
        }
        
        _systemStatistics.CpuPercentIdleTime = 100.0 - (_systemStatistics.CpuPercentUserTime + _systemStatistics.CpuPercentKernelTime);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        SafelyDisposeCancellationTokenSource(_cancellationTokenSource);
        
        _cancellationTokenSource = new CancellationTokenSource();
        
        var processThread = new Thread(() => RunProcessLoop(_cancellationTokenSource.Token));
        processThread.Start();
        
        var renderThread = new Thread(() => RunRenderLoop(_cancellationTokenSource.Token));
        renderThread.Start();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        
        _cancellationTokenSource?.Cancel();
    }

    private void RunRenderLoop(CancellationToken token)
    {
        _systemInfo.GetSystemInfo(_systemStatistics);
        _systemInfo.GetSystemMemory(_systemStatistics);

        while (false == token.IsCancellationRequested) {
            GetTotalSystemTimes();
            UpdateColumnHeaders();
            UpdateListViewItems();
            Draw();

            var startTime = DateTime.Now;

            while (true) {
                ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
                bool handled = _listView.GetInput(ref keyInfo);

                if (false == handled) {
                    var duration = DateTime.Now - startTime;
                    if (duration.TotalMilliseconds >= Processes.UpdateTimeInMs) {
                        break;
                    }
                }
                
                Thread.Sleep(30);
            }
        }
    }
    
    private void RunProcessLoop(CancellationToken token)
    {
        while (false == token.IsCancellationRequested) {
            /*
             * This function runs on a worker thread. The allProcesses array is cloned
             * into the member array _allProcesses for thread-safe access to the data.
             */
            var allProcesses = _processes.GetAll();

            _systemStatistics.ProcessCount = _processes.ProcessCount;
            _systemStatistics.ThreadCount = _processes.ThreadCount;
            _systemStatistics.GhostProcessCount = _processes.GhostProcessCount;

            if (allProcesses.Length == 0) {
                continue;
            }
            
            lock (_processLock) {
                
                Array.Clear(
                    array: _allProcesses, 
                    index: 0, 
                    length: _allProcesses.Length);
                
                Array.Resize(ref _allProcesses, allProcesses.Length);

                Array.Copy(
                    sourceArray: allProcesses, 
                    sourceIndex: 0, 
                    destinationArray: _allProcesses, 
                    destinationIndex: 0, 
                    length: _allProcesses.Length);
            }
        }
    }
    
    private void SafelyDisposeCancellationTokenSource(CancellationTokenSource? cancellationTokenSource)
    {
        try {
            cancellationTokenSource?.Dispose();
        }
        catch (Exception ex) {
            Debug.WriteLine($"Failed SafelyDisposeCancellationTokenSource(): {ex}");            
        }
    }

    private Theme Theme { get; }

    private void UpdateColumnHeaders()
    {
        if (_cachedTerminalWidth == Terminal.WindowWidth) {
            return;
        }
#if __APPLE__
        /* Bug on MacOS where ProcessName returns truncated 15 char value. */
        _listView.ColumnHeaders[(int)Columns.Process].Width = 16;
#elif __WIN32__ 
        _listView.ColumnHeaders[(int)Columns.Process].Width = COLUMN_PROCESS_WIDTH;
#endif
        _listView.ColumnHeaders[(int)Columns.Pid].Width = COLUMN_PID_WIDTH;
        _listView.ColumnHeaders[(int)Columns.User].Width = COLUMN_USER_WIDTH;
        _listView.ColumnHeaders[(int)Columns.Priority].Width = COLUMN_PRIORITY_WIDTH;
        _listView.ColumnHeaders[(int)Columns.Priority].RightAligned = true;
        _listView.ColumnHeaders[(int)Columns.Cpu].Width = COLUMN_CPU_WIDTH;
        _listView.ColumnHeaders[(int)Columns.Cpu].RightAligned = true;
        _listView.ColumnHeaders[(int)Columns.Threads].Width = COLUMN_THREADS_WIDTH;
        _listView.ColumnHeaders[(int)Columns.Threads].RightAligned = true;
        _listView.ColumnHeaders[(int)Columns.Memory].Width = COLUMN_MEMORY_WIDTH;
        _listView.ColumnHeaders[(int)Columns.Memory].RightAligned = true;

        int total =
            COLUMN_PROCESS_WIDTH + COLUMN_MARGIN +
            COLUMN_PID_WIDTH + COLUMN_MARGIN +
            COLUMN_USER_WIDTH + COLUMN_MARGIN +
            COLUMN_PRIORITY_WIDTH + COLUMN_MARGIN +
            COLUMN_CPU_WIDTH + COLUMN_MARGIN +
            COLUMN_THREADS_WIDTH + COLUMN_MARGIN +
            COLUMN_MEMORY_WIDTH + COLUMN_MARGIN;

        if (total + COLUMN_COMMANDLINE_WIDTH + COLUMN_MARGIN < Terminal.WindowWidth) {
            _listView.ColumnHeaders[(int)Columns.CommandLine].Width = Terminal.WindowWidth - total - COLUMN_MARGIN;    
        }
        else {
            _listView.ColumnHeaders[(int)Columns.CommandLine].Width = COLUMN_COMMANDLINE_WIDTH;
        }

        for (int i = 0; i < (int)Columns.Count; i++) {
            _listView.ColumnHeaders[i].BackgroundColour = Theme.HeaderBackground;
            _listView.ColumnHeaders[i].ForegroundColour = Theme.HeaderForeground;
        }
        
        _cachedTerminalWidth = Terminal.WindowWidth;
    }
    
    private void UpdateListViewItems()
    {
        lock (_processLock) {

            var sortedProcesses = _allProcesses
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
}
