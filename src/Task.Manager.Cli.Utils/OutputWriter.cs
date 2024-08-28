using Task.Manager.System;

namespace Task.Manager.Cli.Utils;

public class OutputWriter
{
    private readonly ISystemTerminal? _systemTerminal;
    private readonly object _syncObj = new object();

    public OutputWriter(ISystemTerminal? systemTerminal) => 
        _systemTerminal = systemTerminal;
    
    public void Write(string message)
    {
        UseLock(() =>
        {
            _systemTerminal?.StdOut?.Write(message);
        });
    }

    public void WriteLine()
    {
        UseLock(() =>
        {
            _systemTerminal?.StdOut?.WriteLine();
        });
    }

    public void WriteLine(string message)
    {
        UseLock(() =>
        {
            _systemTerminal?.StdOut?.WriteLine(message);
        });
    }
    
    public void WriteLine(string format, params object?[] args)
    {
        UseLock(() =>
        {
            _systemTerminal?.StdOut?.WriteLine(string.Format(format, args));
        });
    }

    private void UseLock(Action action)
    {
        lock (_syncObj) {
            try {
                action();
            }
            catch (Exception e) {
                _systemTerminal?.StdError?.WriteLine(e);
            }
        }
    }
}
