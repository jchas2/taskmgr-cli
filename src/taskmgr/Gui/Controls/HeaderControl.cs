using System.Drawing;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public sealed class HeaderControl : Control
{
    private readonly IProcessor _processor;
    private const int MetreWidth = 32;

    public HeaderControl(IProcessor processor, ISystemTerminal terminal) : base(terminal)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    protected override void OnDraw()
    {
        var systemStatistics = _processor.SystemStatistics;
        int nlines = 0;
        
        Terminal.SetCursorPosition(left: X, top: Y);
        Terminal.BackgroundColor = MenubarColour;
        Terminal.ForegroundColor = ForegroundColour;

        string menubar = "Task Manager CLI";
        int offsetX = Terminal.WindowWidth / 2 - menubar.Length / 2;
        
        Terminal.WriteEmptyLineTo(offsetX);
        Terminal.Write(menubar);
        Terminal.WriteEmptyLineTo(Width - offsetX - menubar.Length);
        
        nlines++;

        Terminal.BackgroundColor = BackgroundColour;

        Terminal.Write(
            $"{systemStatistics.MachineName}  ({systemStatistics.OsVersion})  IP {systemStatistics.PrivateIPv4Address} Pub {systemStatistics.PublicIPv4Address}");
        
        int nchars =
            systemStatistics.MachineName.Length + 3 +
            systemStatistics.OsVersion.Length + 6 +
            systemStatistics.PrivateIPv4Address.Length + 5 +
            systemStatistics.PublicIPv4Address.Length;
        
        Terminal.WriteEmptyLineTo(Width - nchars);
        
        nlines++;
        
        Terminal.Write(
            $"{systemStatistics.CpuName} (Cores {systemStatistics.CpuCores})");

        nchars =
            systemStatistics.CpuName.Length + 8 +
            systemStatistics.CpuCores.ToString().Length + 1;

        Terminal.WriteEmptyLineTo(Width - nchars);
        Terminal.WriteEmptyLine();
        
        nlines += 2;

        double totalCpu = systemStatistics.CpuPercentKernelTime + systemStatistics.CpuPercentUserTime;
        double memRatio = 1.0 - ((double)(systemStatistics.AvailablePhysical) / (double)(systemStatistics.TotalPhysical));
        double virRatio = 1.0 - ((double)(systemStatistics.AvailablePageFile) / (double)(systemStatistics.TotalPageFile));
        
        var userColour = totalCpu < 50.0 ? ConsoleColor.DarkGreen
            : totalCpu < 75.0 ? ConsoleColor.DarkYellow
            : ConsoleColor.Red;

        var kernelColour = totalCpu < 50.0 ? ConsoleColor.Green
            : totalCpu < 75.0 ? ConsoleColor.Yellow
            : ConsoleColor.DarkRed;

        var memColour = memRatio < 0.5 ? ConsoleColor.DarkGreen
            : memRatio < 0.75 ? ConsoleColor.DarkYellow
            : ConsoleColor.Red;

        var virColour = virRatio < 0.5 ? ConsoleColor.DarkGreen
            : virRatio < 0.75 ? ConsoleColor.DarkYellow
            : ConsoleColor.Red;

        Terminal.Write("Cpu ");

        nchars = DrawStackedPercentageBar(
            "k",
            systemStatistics.CpuPercentKernelTime / 100,
            kernelColour,
            "u",
            systemStatistics.CpuPercentUserTime / 100,
            userColour);
        
        nchars += DrawColumnLabelValue(
            "  Cpu:     ",
            (totalCpu / 100).ToString("000.0%"),
            userColour);
        
        nchars += DrawColumnLabelValue(
            "  Mem:     ",
            memRatio.ToString("000.0%"),
            memColour);
        
        nchars += DrawColumnLabelValue(
            "  Virt:    ",
            virRatio.ToString("000.0%"),
            virColour);
        
        Terminal.WriteEmptyLineTo(Width - nchars - 4);
        
        nlines++;
        
        Terminal.Write("Mem ");

        nchars = DrawPercentageBar(
            "m",
            memRatio,
            memColour);

        nchars += DrawColumnLabelValue(
            "  User:    ",
            (systemStatistics.CpuPercentUserTime / 100).ToString("000.0%"),
            userColour);

        nchars += DrawColumnLabelValue(
            "  Total: ",
            ((double)(systemStatistics.TotalPhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        nchars += DrawColumnLabelValue(
            "  Total: ",
            ((double)(systemStatistics.TotalPageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Width - nchars - 4);

        nlines++;
        
        Terminal.Write("Vir ");

        nchars = DrawPercentageBar(
            "v",
            virRatio,
            virColour);

        nchars += DrawColumnLabelValue(
            "  Kernel:  ",
            (systemStatistics.CpuPercentKernelTime / 100).ToString("000.0%"),
            kernelColour);

        nchars += DrawColumnLabelValue(
            "  Used:  ",
            ((double)(systemStatistics.TotalPhysical - systemStatistics.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        nchars += DrawColumnLabelValue(
            "  Used:  ",
            ((double)(systemStatistics.TotalPageFile - systemStatistics.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Width - nchars - 4);

        nlines++;
        nchars = 4 + 1 + MetreWidth + 1;

        Terminal.WriteEmptyLineTo(nchars);

        nlines++;
        
        nchars += DrawColumnLabelValue(
            "  Idle:    ",
            (systemStatistics.CpuPercentIdleTime / 100).ToString("000.0%"),
            ForegroundColour);

        nchars += DrawColumnLabelValue(
            "  Free:  ",
            ((double)(systemStatistics.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        nchars += DrawColumnLabelValue(
            "  Free:  ",
            ((double)(systemStatistics.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Width - nchars);
        
        nlines++;
        
        nchars = DrawColumnLabelValue(
            "Processes: ",
            systemStatistics.ProcessCount.ToString(),
            ForegroundColour);
#if DEBUG        
        nchars += DrawColumnLabelValue(
            ", Ghosts: ",
            systemStatistics.GhostProcessCount.ToString(),
            ForegroundColour);
#endif
        nchars += DrawColumnLabelValue(
            ", Threads: ",
            systemStatistics.ThreadCount.ToString(),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Width - nchars);
        
        Height = nlines;
    }

    private int DrawColumnLabelValue(
        string label,
        string value,
        ConsoleColor valueColour)
    {
        Terminal.ForegroundColor = ForegroundColour;
        Terminal.Write(label);
        Terminal.ForegroundColor = valueColour;
        Terminal.Write(value);
        Terminal.ForegroundColor = ForegroundColour;

        return label.Length + value.Length;
    }

    private int DrawStackedPercentageBar(
        string labelA,
        double percentageA,
        ConsoleColor colourA,
        string labelB,
        double percentageB,
        ConsoleColor colourB)
    {
        int nchars = 0;
        
        Terminal.BackgroundColor = BackgroundColour;
        Terminal.ForegroundColor = ForegroundColour;
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
        ConsoleColor colour)
    {
        int nchars = 0;
        
        Terminal.BackgroundColor = BackgroundColour;
        Terminal.ForegroundColor = ForegroundColour;
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
        Terminal.ForegroundColor = currForegroundColour;

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

            for (int i = 0; i < MetreWidth - (inverseBars + offsetX); i++) {
                Terminal.Write(' ');
                nchars++;
            }
        }
        
        Terminal.BackgroundColor = currBackgroundColour;
        Terminal.ForegroundColor = currForegroundColour;
        
        return nchars;
    }
    
    public ConsoleColor MenubarColour { get; set; } = ConsoleColor.DarkGray;
}
