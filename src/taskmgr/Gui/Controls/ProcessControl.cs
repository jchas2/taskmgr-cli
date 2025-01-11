using System;
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
    
    public ProcessControl(ISystemTerminal terminal, IProcessor processor)
        : base(terminal)
    {
        _allProcesses = [];
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    public ProcessInfo[] GetProcesses()
    {
        lock (_processLock) {
            return _allProcesses;
        }
    }

    private void RunIOLoop()
    {
        
    }
    
    private void RunProcessLoop()
    {
        while (true) {
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
}
