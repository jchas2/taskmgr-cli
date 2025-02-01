using System.Drawing;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Controls;

namespace Task.Manager.Gui;

public sealed class SystemHeaderView : Control
{
    private readonly Config _config;
    private readonly Theme _theme;

    private const int MetreWidth = 32;

    public SystemHeaderView(
        ISystemTerminal terminal, 
        Config config,
        Theme theme)
    : base(terminal)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    public void Draw(
        SystemStatistics systemStats,
        SystemTimes systemTimes,
        ref Rectangle bounds)
    {
        int nlines = 0;
        
        Terminal.SetCursorPosition(left: 0, top: 0);
        Terminal.BackgroundColor = _theme.Menubar;
        Terminal.ForegroundColor = _theme.Foreground;

        string menubar = "Task Manager CLI";
        int offsetX = Terminal.WindowWidth / 2 - menubar.Length / 2;
        
        Terminal.WriteEmptyLineTo(offsetX);
        Terminal.Write(menubar);
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - offsetX - menubar.Length);
        
        nlines += 2;

        Terminal.BackgroundColor = _theme.Background;

        Terminal.Write(
            $"{systemStats.MachineName}  ({systemStats.OsVersion})  IP {systemStats.PrivateIPv4Address} Pub {systemStats.PublicIPv4Address}");
        
        int nchars =
            systemStats.MachineName.Length + 3 +
            systemStats.OsVersion.Length + 6 +
            systemStats.PrivateIPv4Address.Length + 5 +
            systemStats.PublicIPv4Address.Length;
        
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars);
        
        nlines++;
        
        Terminal.Write(
            $"{systemStats.CpuName} (Cores {systemStats.CpuCores})");

        nchars =
            systemStats.CpuName.Length + 8 +
            systemStats.CpuCores.ToString().Length + 1;

        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars);
        Terminal.WriteEmptyLine();
        
        nlines += 2;

        long totalCpu = systemTimes.Kernel + systemTimes.User;
        double memRatio = 1.0 - ((double)(systemStats.AvailablePhysical) / (double)(systemStats.TotalPhysical));
        double virRatio = 1.0 - ((double)(systemStats.AvailablePageFile) / (double)(systemStats.TotalPageFile));
        
        var userColour = totalCpu < 50 ? ConsoleColor.DarkGreen
            : totalCpu < 75 ? ConsoleColor.DarkYellow
            : ConsoleColor.Red;

        var kernelColour = totalCpu < 50 ? ConsoleColor.Green
            : totalCpu < 75 ? ConsoleColor.Yellow
            : ConsoleColor.DarkRed;

        var memColour = memRatio < 0.5 ? ConsoleColor.DarkGreen
            : memRatio < 0.75 ? ConsoleColor.DarkYellow
            : ConsoleColor.Red;

        var virColour = virRatio < 0.5 ? ConsoleColor.DarkGreen
            : virRatio < 0.75 ? ConsoleColor.DarkYellow
            : ConsoleColor.Red;

        Terminal.Write("Cpu ");

        nchars += DrawStackedPercentageBar(
            "k",
            (double)(systemTimes.Kernel) / 100,
            kernelColour,
            "u",
            (double)(systemTimes.User) / 100,
            userColour,
            _theme);
        
        nchars += DrawColumnLabelValue(
            "  Cpu:     ",
            ((double)(systemTimes.Kernel + systemTimes.User) * 100 / 100).ToString("000.0%"),
            userColour,
            _theme);
        
        nchars += DrawColumnLabelValue(
            "  Mem:     ",
            (memRatio * 100).ToString("000.0%"),
            memColour,
            _theme);
        
        nchars += DrawColumnLabelValue(
            "  Virt:    ",
            (virRatio * 100).ToString("000.0%"),
            virColour,
            _theme);
        
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars - 4);
        
        nlines++;
        
        Terminal.Write("Mem ");

        nchars += DrawPercentageBar(
            "m",
            memRatio,
            memColour,
            _theme);

        nchars += DrawColumnLabelValue(
            "  User:    ",
            systemTimes.User.ToString("000.0%"),
            userColour,
            _theme);

        nchars += DrawColumnLabelValue(
            "  Total: ",
            ((double)(systemStats.TotalPhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            _theme);
        
        nchars += DrawColumnLabelValue(
            "  Total: ",
            ((double)(systemStats.TotalPageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            _theme);
        
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars - 4);

        nlines++;
        
        Terminal.Write("Vir ");

        nchars += DrawPercentageBar(
            "v",
            virRatio,
            virColour,
            _theme);

        nchars += DrawColumnLabelValue(
            "  Kernel: ",
            ((double)(systemTimes.Kernel)).ToString("000.0%"),
            kernelColour,
            _theme);

        nchars += DrawColumnLabelValue(
            "  Used:  ",
            ((double)(systemStats.TotalPhysical - systemStats.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            _theme);
        
        nchars += DrawColumnLabelValue(
            "  Used:  ",
            ((double)(systemStats.TotalPageFile - systemStats.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            _theme);
        
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars - 4);

        nlines++;
        nchars = 4 + 1 + MetreWidth + 1;

        Terminal.WriteEmptyLineTo(nchars);
        
        nlines++;
        
        nchars += DrawColumnLabelValue(
            "  Idle:   ",
            systemTimes.Idle.ToString("000.0%"),
            _theme);

        nchars += DrawColumnLabelValue(
            "  Free:  ",
            ((double)(systemStats.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            _theme);
        
        nchars += DrawColumnLabelValue(
            "  Free:  ",
            ((double)(systemStats.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            _theme);
        
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars);
        
        bounds.Y = nlines;
    }

    private int DrawColumnLabelValue(
        string label, 
        string value, 
        Theme theme) =>
            DrawColumnLabelValue(
                label,
                value,
                theme.Foreground,
                theme);

    private int DrawColumnLabelValue(
        string label,
        string value,
        ConsoleColor valueColour,
        Theme theme)
    {
        Terminal.ForegroundColor = theme.Foreground;
        Terminal.Write(label);
        Terminal.ForegroundColor = valueColour;
        Terminal.Write(value);
        Terminal.ForegroundColor = theme.Foreground;

        return label.Length + value.Length;
    }

    private int DrawStackedPercentageBar(
        string labelA,
        double percentageA,
        ConsoleColor colourA,
        string labelB,
        double percentageB,
        ConsoleColor colourB,
        Theme theme)
    {
        int nchars = 0;
        
        Terminal.BackgroundColor = theme.Background;
        Terminal.ForegroundColor = theme.Foreground;
        Terminal.Write('[');

        nchars++;

        nchars += DrawMeter(
            labelA,
            percentageA,
            colourA,
            offsetX: 0,
            drawVoid: false);
        
        nchars += DrawMeter(
            labelB,
            percentageB,
            colourB,
            offsetX: nchars - 1,
            drawVoid: true);
        
        Terminal.Write(']');

        return ++nchars;
    }
    
    private int DrawPercentageBar(
        string label,
        double percentage,
        ConsoleColor colour,
        Theme theme)
    {
        int nchars = 0;
        
        Terminal.BackgroundColor = theme.Background;
        Terminal.ForegroundColor = theme.Foreground;
        Terminal.Write('[');

        nchars++;

        nchars += DrawMeter(
            label,
            percentage,
            colour,
            offsetX: 0,
            drawVoid: true);
        
        Terminal.Write(']');

        return ++nchars;
    }
    
    private int DrawMeter(
        string label,
        double percentage,
        ConsoleColor colour,
        int offsetX,
        bool drawVoid)
    {
        int nchars = 0;
        int inverseBars = (int)(percentage * (double)MetreWidth);

        if (inverseBars == 0 && percentage > 0) {
            inverseBars = 1;
        }

        var currBackgroundColour = Terminal.BackgroundColor;
        var currForegroundColour = Terminal.ForegroundColor;
        
        Terminal.BackgroundColor = colour;
        Terminal.ForegroundColor = colour;

        if (inverseBars >= label.Length) {
            Terminal.Write(label);
            nchars += label.Length;
        }

        for (int i = nchars; i < inverseBars; i++) {
            Terminal.Write(' ');
            nchars++;
        }

        if (drawVoid)
        {
            /* TODO: Needs to be a theme colour for this. */
            Terminal.BackgroundColor = ConsoleColor.DarkGray;
            Terminal.ForegroundColor = ConsoleColor.DarkGray;

            for (int i = 0; i < MetreWidth - (inverseBars + offsetX); i++){
                Terminal.Write(' ');
            }
            
            nchars += MetreWidth - (inverseBars + offsetX);
        }
        
        Terminal.BackgroundColor = currBackgroundColour;
        Terminal.ForegroundColor = currForegroundColour;
        
        return nchars;
    }
}
