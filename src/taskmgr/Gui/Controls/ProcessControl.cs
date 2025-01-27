using System.Diagnostics;
using System.Runtime.InteropServices;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public class ProcessControl : Control
{
    private ProcessInfo[] _allProcesses;
    private readonly object _processLock = new();
    private readonly IProcessor _processor;
    private readonly ISystemInfo _systemInfo;
    private CancellationTokenSource? _cancellationTokenSource;
    
    public ProcessControl( ISystemTerminal terminal, IProcessor processor, ISystemInfo systemInfo)
        : base(terminal)
    {
        _allProcesses = [];
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
    }

    private void GetTotalSystemTimes(ref SystemStatistics systemStatistics)
    {
        systemStatistics.CpuPercentIdleTime = 0.0;
        systemStatistics.CpuPercentUserTime = 0.0;
        systemStatistics.CpuPercentKernelTime = 0.0;
        
        lock (_processLock) {
            for (int i = 0; i < _allProcesses.Length; i++) {
                systemStatistics.CpuPercentUserTime += _allProcesses[i].ProcessorUserTime;
                systemStatistics.CpuPercentKernelTime += _allProcesses[i].ProcessorKernelTime;
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
        
        processThread.Join();
        renderThread.Join();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        
        _cancellationTokenSource?.Cancel();
    }

    private void RunRenderLoop(CancellationToken token)
    {
        SystemStatistics systemStatistics = new();
        
        _systemInfo.GetSystemInfo(ref systemStatistics);
        _systemInfo.GetSystemMemory(ref systemStatistics);

        while (true) {
            GetTotalSystemTimes(ref systemStatistics);
            // Draw();
            
            // Handle input.
        }
    }
    
    private void RunProcessLoop(CancellationToken token)
    {
        while (false == token.IsCancellationRequested) {
            /*
             * This function runs on a worker thread. The allProcesses array is cloned
             * into the member array _allProcesses for thread-safe access to the data.
             */
            var allProcesses = _processor.GetProcesses();

            if (allProcesses.Length == 0) {
                continue;
            }
            
            lock (_processLock) {
                
                Array.Clear(
                    array: _allProcesses, 
                    index: 0, 
                    length: _allProcesses.Length);
                
                Array.Resize(ref _allProcesses, allProcesses.Length);
                
                /*
                 *  It's important ProcessInfo is defined as a value-type for the following
                 *  BlockCopy to deep copy.
                 */
                
                int bytes = allProcesses.Length * Marshal.SizeOf<ProcessInfo>();
                
                Buffer.BlockCopy(
                    src: allProcesses, 
                    srcOffset: 0, 
                    dst: _allProcesses, 
                    dstOffset: 0, 
                    count: bytes);
            }
        }
    }

    private void SafelyDisposeCancellationTokenSource(CancellationTokenSource? cancellationTokenSource)
    {
        if (null == cancellationTokenSource) {
            return;
        }

        try {
            cancellationTokenSource.Dispose();
        }
        catch (Exception ex) {
            Debug.WriteLine($"Failed SafelyDisposeCancellationTokenSource(): {ex}");            
        }
    }
}
