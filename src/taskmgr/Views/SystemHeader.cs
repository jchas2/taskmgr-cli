using System.Drawing;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Configuration;

namespace Task.Manager.Views;

public sealed class SystemHeader
{
    private readonly ISystemTerminal _terminal;
    private readonly Config _config;
    private readonly Theme _theme;

    public SystemHeader(
        ISystemTerminal terminal, 
        Config config,
        Theme theme)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    public void Draw(
        SystemStatistics systemStatistics,
        SystemTimes systemTimes,
        Rectangle bounds)
    {
        int nlines = 0;
        
        _terminal.SetCursorPosition(left: 0, top: 0);
        _terminal.BackgroundColor = _theme.Menubar;
        _terminal.ForegroundColor = _theme.Foreground;

        string menubar = "Task Manager CLI";
        int offsetX = _terminal.WindowWidth / 2 - menubar.Length / 2;
        
        _terminal.WriteEmptyLineTo(offsetX);
        _terminal.Write(menubar);
        _terminal.WriteEmptyLineTo(_terminal.WindowWidth - offsetX - menubar.Length);
        nlines += 2;

        _terminal.BackgroundColor = _theme.Background;
        _terminal.Write(systemStatistics.MachineName);
        _terminal.Write("  (");
        _terminal.Write(systemStatistics.OsVersion);
        _terminal.Write(")  IP ");
        _terminal.Write(systemStatistics.PrivateIPv4Address);
        _terminal.Write(" Pub ");
        _terminal.Write(systemStatistics.PublicIPv4Address);

        int nchars =
            systemStatistics.MachineName.Length + 3 +
            systemStatistics.OsVersion.Length + 6 +
            systemStatistics.PrivateIPv4Address.Length + 5 +
            systemStatistics.PublicIPv4Address.Length;
        
        _terminal.WriteEmptyLineTo(_terminal.WindowWidth - nchars);
        nlines++;
        
        _terminal.Write(systemStatistics.CpuName);
        _terminal.Write(" (Cores ");
        _terminal.Write(systemStatistics.CpuCores.ToString());
        _terminal.Write(")");

        nchars =
            systemStatistics.CpuName.Length + 8 +
            systemStatistics.CpuCores.ToString().Length + 1;

        _terminal.WriteEmptyLineTo(_terminal.WindowWidth - -nchars);
        _terminal.WriteEmptyLine();
        nlines += 2;
    }
}