using System.Drawing;
using Task.Manager.System;
using Task.Manager.System.Configuration;

namespace Task.Manager.Views;

public sealed class SystemHeader
{
    private readonly ISystemTerminal _terminal;
    private readonly Config _config;

    public SystemHeader(ISystemTerminal terminal, Config config)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public void Draw(
        SystemStatistics systemStatistics,
        SystemTimes systemTimes,
        Rectangle bounds)
    {
        //int n = 0;
        
        _terminal.SetCursorPosition(left: 0, top: 0);
        
        
    }
}