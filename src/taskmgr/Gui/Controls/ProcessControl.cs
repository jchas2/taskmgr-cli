using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public sealed class ProcessControl : Control
{
    private class ProcessListViewItem : ListViewItem
    {
        private ProcessInfo _processInfo;

        public ProcessListViewItem(
            ProcessInfo processInfo,
            ConsoleColor backgroundColor,
            ConsoleColor foregroundColor)
            : base(processInfo.ExeName ?? string.Empty, backgroundColor, foregroundColor)
        {
            var userSubItem = new ListViewSubItem(this, processInfo.UserName ?? string.Empty);
            var priSubItem = new ListViewSubItem(this, processInfo.BasePriority.ToString());
            var cpuSubItem = new ListViewSubItem(this, (processInfo.CpuTimePercent / 100).ToString("00.00%", CultureInfo.InvariantCulture));
        
            SubItems.Add(userSubItem);
            SubItems.Add(priSubItem);
            SubItems.Add(cpuSubItem);
            
            _processInfo = processInfo;
        }

        public ProcessInfo ProcessInfo
        {
            get => _processInfo;
            set {
                _processInfo = value;
                SubItems[0].Text = _processInfo.ExeName ?? string.Empty;
                SubItems[1].Text = _processInfo.BasePriority.ToString();
                SubItems[2].Text = (_processInfo.CpuTimePercent / 100).ToString("00.00%", CultureInfo.InvariantCulture);
            } 
        }
    }
    
    private ProcessInfo[] _allProcesses;
    private readonly object _processLock = new();
    private readonly IProcesses _processes;
    private readonly ISystemInfo _systemInfo;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly SystemStatistics _systemStatistics;

    private readonly HeaderControl _headerControl;
    private readonly ListView _listView;
    
    public ProcessControl(ISystemTerminal terminal, IProcesses processes, ISystemInfo systemInfo)
        : base(terminal)
    {
        _allProcesses = [];
        _processes = processes ?? throw new ArgumentNullException(nameof(processes));
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
        _systemStatistics = new SystemStatistics();
        
        _headerControl = new HeaderControl(Terminal);
        
        _listView = new ListView(Terminal);
        _listView.BackgroundHighlightColour = ConsoleColor.Cyan;
        _listView.ForegroundHighlightColour = ConsoleColor.Black;
        _listView.BackgroundColour = ConsoleColor.Black;
        _listView.ForegroundColour = ConsoleColor.White;
        
        Controls.Add(_headerControl);
        Controls.Add(_listView);
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
            UpdateListViewItems();
            Draw();

            var startTime = DateTime.Now;

            while (true) {
                // TODO: Handle input (up + down arrows etc. Simulate it here with a thread sleep.
                Thread.Sleep(500);
                var duration = DateTime.Now - startTime;
                if (duration.TotalMilliseconds >= 1000) {
                    break;
                }
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

    private void UpdateListViewItems()
    {
        lock (_processLock) {
            if (_listView.Items.Count == 0) {
                
                for (int i = 0; i < _allProcesses.Length; i++) {
                    var item = new ProcessListViewItem(_allProcesses[i], BackgroundColour, ForegroundColour);
                    _listView.Items.Add(item);
                }
                
                return;
            }

            for (int i = 0; i < _allProcesses.Length; i++) {
                bool found = false;
                
                for (int j = 0; j < _listView.Items.Count; j++) {
                    var item = _listView.Items[j] as ProcessListViewItem;
                    
                    if (item == null) {
                        continue;
                    }
                    
                    if (_allProcesses[i].Pid == item.ProcessInfo.Pid) {
                        item.ProcessInfo = _allProcesses[i];
                        found = true;
                        break;
                    }
                }

                if (false == found) {
                    var item = new ProcessListViewItem(_allProcesses[i], BackgroundColour, ForegroundColour);
                    _listView.Items.Add(item);
                }
            }
        }
    }
}
