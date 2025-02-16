using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public sealed class ProcessControl : Control
{
    private ProcessInfo[] _allProcesses;
    private readonly object _processLock = new();
    private readonly IProcesses _processes;
    private readonly ISystemInfo _systemInfo;
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly HeaderControl _headerControl;
    
    public ProcessControl(ISystemTerminal terminal, IProcesses processes, ISystemInfo systemInfo)
        : base(terminal)
    {
        _allProcesses = [];
        _processes = processes ?? throw new ArgumentNullException(nameof(processes));
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
        
        _headerControl = new HeaderControl(Terminal);
        Controls.Add(_headerControl);
    }

    private void Draw(SystemStatistics statistics)
    {
        var bounds = new Rectangle(
            x: 0, 
            y: 0, 
            Terminal.WindowWidth, 
            Terminal.WindowHeight);
        
        _headerControl.Draw(statistics, ref bounds);
    }
    
    private void GetTotalSystemTimes(SystemStatistics systemStatistics)
    {
        systemStatistics.CpuPercentIdleTime = 0.0;
        systemStatistics.CpuPercentUserTime = 0.0;
        systemStatistics.CpuPercentKernelTime = 0.0;
        
        lock (_processLock) {
            for (int i = 0; i < _allProcesses.Length; i++) {
                systemStatistics.CpuPercentUserTime += _allProcesses[i].CpuUserTimePercent;
                systemStatistics.CpuPercentKernelTime += _allProcesses[i].CpuKernelTimePercent;
            }
        }
        
        systemStatistics.CpuPercentIdleTime = 100.0 - (systemStatistics.CpuPercentUserTime + systemStatistics.CpuPercentKernelTime);
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
        SystemStatistics systemStatistics = new();
        
        _systemInfo.GetSystemInfo(systemStatistics);
        _systemInfo.GetSystemMemory(systemStatistics);

        while (false == token.IsCancellationRequested) {
            GetTotalSystemTimes(systemStatistics);
            Draw(systemStatistics);

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
}
