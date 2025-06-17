using System.Diagnostics;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ModulesControl : Control
{
    private readonly Theme _theme;
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly ListView _listView;

    private int _cachedTerminalWidth = 0;
    
    public ModulesControl(
        ISystemTerminal terminal, 
        Theme theme)
        : base(terminal)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        
        _listView = new ListView(terminal);
        
        Controls.Add(_listView);
        
        Theme = theme;
    }

    private void Draw()
    {
        
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();
        
        _listView.BackgroundHighlightColour = _theme.BackgroundHighlight;
        _listView.ForegroundHighlightColour = _theme.ForegroundHighlight;
        _listView.BackgroundColour = _theme.Background;
        _listView.ForegroundColour = _theme.Foreground;
        _listView.HeaderBackgroundColour = _theme.HeaderBackground;
        _listView.HeaderForegroundColour = _theme.HeaderForeground;
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("MODULE"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("FILENAME"));
        _listView.X = 0;
        _listView.Y = 0;
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
        var moduleInfos = ModuleInfo.GetModules(0);

        if (moduleInfos.Length == 0) {
            
        }
        
        while (false == token.IsCancellationRequested) {
            UpdateColumnHeaders();
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
                if (duration.TotalMilliseconds >= 1000) {
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
// #if __APPLE__
//         /* Bug on MacOS where ProcessName returns truncated 15 char value. */
//         _listView.ColumnHeaders[(int)Columns.Process].Width = 16;
// #elif __WIN32__ 
//         _listView.ColumnHeaders[(int)Columns.Process].Width = ColumnProcessWidth;
// #endif
//         _listView.ColumnHeaders[(int)Columns.Pid].Width = ColumnPidWidth;
//         _listView.ColumnHeaders[(int)Columns.User].Width = ColumnUserWidth;
//         _listView.ColumnHeaders[(int)Columns.Priority].Width = ColumnPriorityWidth;
//         _listView.ColumnHeaders[(int)Columns.Priority].RightAligned = true;
//         _listView.ColumnHeaders[(int)Columns.Cpu].Width = ColumnCpuWidth;
//         _listView.ColumnHeaders[(int)Columns.Cpu].RightAligned = true;
//         _listView.ColumnHeaders[(int)Columns.Threads].Width = ColumnThreadsWidth;
//         _listView.ColumnHeaders[(int)Columns.Threads].RightAligned = true;
//         _listView.ColumnHeaders[(int)Columns.Memory].Width = ColumnMemoryWidth;
//         _listView.ColumnHeaders[(int)Columns.Memory].RightAligned = true;
//
//         int total =
//             ColumnProcessWidth + ColumnMargin +
//             ColumnPidWidth + ColumnMargin +
//             ColumnUserWidth + ColumnMargin +
//             ColumnPriorityWidth + ColumnMargin +
//             ColumnCpuWidth + ColumnMargin +
//             ColumnThreadsWidth + ColumnMargin +
//             ColumnMemoryWidth + ColumnMargin;
//
//         if (total + ColumnCommandlineWidth + ColumnMargin < Width) {
//             _listView.ColumnHeaders[(int)Columns.CommandLine].Width = Width - total - ColumnMargin;    
//         }
//         else {
//             _listView.ColumnHeaders[(int)Columns.CommandLine].Width = ColumnCommandlineWidth;
//         }
//
//         for (int i = 0; i < (int)Columns.Count; i++) {
//             _listView.ColumnHeaders[i].BackgroundColour = Theme.HeaderBackground;
//             _listView.ColumnHeaders[i].ForegroundColour = Theme.HeaderForeground;
//         }
        
        _cachedTerminalWidth = Terminal.WindowWidth;
    }

}