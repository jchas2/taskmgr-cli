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
    private readonly IProcessor _processor;
    private readonly Theme _theme;
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly HeaderControl _headerControl;
    private readonly ListView _listView;

    private int _cachedTerminalWidth = 0;
    
    public ProcessControl(
        ISystemTerminal terminal, 
        IProcessor processor, 
        Theme theme)
        : base(terminal)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        
        _headerControl = new HeaderControl(processor, terminal);
        _listView = new ListView(terminal);
        
        Controls.Add(_headerControl);
        Controls.Add(_listView);
        
        Theme = theme;
    }

    private void Draw()
    {
        _headerControl.X = 0;
        _headerControl.Y = 0;
        _headerControl.Width = Width;
        
        _headerControl.Draw();

        _listView.X = 0;
        _listView.Y = _headerControl.Y + _headerControl.Height + 1;
        _listView.Width = Width;
        _listView.Height = Terminal.WindowHeight - (_headerControl.Height + 1);
        
        _listView.Draw();
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();
        
        _headerControl.BackgroundColour = _theme.Background;
        _headerControl.ForegroundColour = _theme.Foreground;
        _headerControl.MenubarColour = _theme.Menubar;

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
        _listView.X = 0;
        _listView.Y = _headerControl.Y + _headerControl.Height + 1;
        _listView.Width = Terminal.WindowWidth;
        

        SafelyDisposeCancellationTokenSource(_cancellationTokenSource);
        
        _cancellationTokenSource = new CancellationTokenSource();
        
        var renderThread = new Thread(() => RunRenderLoop(_cancellationTokenSource.Token));
        renderThread.Start();
    }

    protected override void OnUnload()
    {
        _cancellationTokenSource?.Cancel();
        base.OnUnload();
    }

    private void RunRenderLoop(CancellationToken token)
    {
        while (false == token.IsCancellationRequested) {
            ProcessInfo[] allProcesses = _processor.GetAll();

            if (allProcesses.Length == 0) {
                continue;
            }

            UpdateColumnHeaders();
            UpdateListViewItems(allProcesses);
            Draw();
            
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
            var startTime = DateTime.Now;
            
            while (true) {
                bool handled = _listView.GetInput(ref keyInfo);

                if (handled) {
                    startTime = DateTime.Now;
                }
                else { 
                    Thread.Sleep(30);
                }
                
                var duration = DateTime.Now - startTime;
                if (duration.TotalMilliseconds >= Processor.UpdateTimeInMs) {
                    break;
                }
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
        
        _cachedTerminalWidth = Terminal.WindowWidth;
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
