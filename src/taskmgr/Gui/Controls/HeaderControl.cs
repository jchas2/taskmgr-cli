using System.Drawing;
using Task.Manager.System;
using Task.Manager.System.Controls;

namespace Task.Manager.Gui.Controls;

public sealed class HeaderControl(ISystemTerminal terminal) : Control(terminal)
{
    private const int MetreWidth = 32;

    public void Draw(SystemStatistics systemStats, ref Rectangle bounds)
    {
        int nlines = 0;
        
        Terminal.SetCursorPosition(left: bounds.X, top: bounds.Y);
        Terminal.BackgroundColor = MenubarColour;
        Terminal.ForegroundColor = ForegroundColour;

        string menubar = "Task Manager CLI";
        int offsetX = Terminal.WindowWidth / 2 - menubar.Length / 2;
        
        Terminal.WriteEmptyLineTo(offsetX);
        Terminal.Write(menubar);
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - offsetX - menubar.Length);
        
        nlines += 2;

        Terminal.BackgroundColor = BackgroundColour;

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

        double totalCpu = systemStats.CpuPercentKernelTime + systemStats.CpuPercentUserTime;
        double memRatio = 1.0 - ((double)(systemStats.AvailablePhysical) / (double)(systemStats.TotalPhysical));
        double virRatio = 1.0 - ((double)(systemStats.AvailablePageFile) / (double)(systemStats.TotalPageFile));
        
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
            (double)(systemStats.CpuPercentKernelTime) / 100,
            kernelColour,
            "u",
            (double)(systemStats.CpuPercentUserTime) / 100,
            userColour);
        
        nchars += DrawColumnLabelValue(
            "  Cpu:     ",
            ((double)(systemStats.CpuPercentKernelTime + systemStats.CpuPercentUserTime) * 100 / 100).ToString("000.0%"),
            userColour);
        
        nchars += DrawColumnLabelValue(
            "  Mem:     ",
            (memRatio * 100).ToString("000.0%"),
            memColour);
        
        nchars += DrawColumnLabelValue(
            "  Virt:    ",
            (virRatio * 100).ToString("000.0%"),
            virColour);
        
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars - 4);
        
        nlines++;
        
        Terminal.Write("Mem ");

        nchars = DrawPercentageBar(
            "m",
            memRatio,
            memColour);

        nchars += DrawColumnLabelValue(
            "  User:    ",
            systemStats.CpuPercentUserTime.ToString("000.0%"),
            userColour);

        nchars += DrawColumnLabelValue(
            "  Total: ",
            ((double)(systemStats.TotalPhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        nchars += DrawColumnLabelValue(
            "  Total: ",
            ((double)(systemStats.TotalPageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars - 4);

        nlines++;
        
        Terminal.Write("Vir ");

        nchars = DrawPercentageBar(
            "v",
            virRatio,
            virColour);

        nchars += DrawColumnLabelValue(
            "  Kernel: ",
            ((double)(systemStats.CpuPercentKernelTime)).ToString("000.0%"),
            kernelColour);

        nchars += DrawColumnLabelValue(
            "  Used:  ",
            ((double)(systemStats.TotalPhysical - systemStats.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        nchars += DrawColumnLabelValue(
            "  Used:  ",
            ((double)(systemStats.TotalPageFile - systemStats.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars - 4);

        nlines++;
        nchars = 4 + 1 + MetreWidth + 1;

        Terminal.WriteEmptyLineTo(nchars);
        
        nlines++;
        
        nchars = DrawColumnLabelValue(
            "  Idle:   ",
            systemStats.CpuPercentIdleTime.ToString("000.0%"),
            ForegroundColour);

        nchars += DrawColumnLabelValue(
            "  Free:  ",
            ((double)(systemStats.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        nchars += DrawColumnLabelValue(
            "  Free:  ",
            ((double)(systemStats.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Terminal.WindowWidth - nchars);
        
        bounds.Y = nlines;
        bounds.X = 0;
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
