using System.Drawing;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public sealed class HeaderControl : Control
{
    private readonly IProcessor _processor;
    private SystemStatistics _systemStatistics = new();
    private const int MetreWidth = 32;

    public HeaderControl(
        IProcessor processor, 
        ISystemTerminal terminal, 
        Theme theme) : base(terminal)
    {
        _processor = processor;
        BackgroundColour = theme.Background;
        ForegroundColour = theme.Foreground;
        MenubarColour = theme.Menubar;
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

    protected override void OnDraw()
    {
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
            $"{_systemStatistics.MachineName}  ({_systemStatistics.OsVersion})  IP {_systemStatistics.PrivateIPv4Address} Pub {_systemStatistics.PublicIPv4Address}");
        
        int nchars =
            _systemStatistics.MachineName.Length + 3 +
            _systemStatistics.OsVersion.Length + 6 +
            _systemStatistics.PrivateIPv4Address.Length + 5 +
            _systemStatistics.PublicIPv4Address.Length;
        
        Terminal.WriteEmptyLineTo(Width - nchars);
        
        nlines++;
        
        Terminal.Write(
            $"{_systemStatistics.CpuName} (Cores {_systemStatistics.CpuCores})");

        nchars =
            _systemStatistics.CpuName.Length + 8 +
            _systemStatistics.CpuCores.ToString().Length + 1;

        Terminal.WriteEmptyLineTo(Width - nchars);
        Terminal.WriteEmptyLine();
        
        nlines += 2;

        double totalCpu = _systemStatistics.CpuPercentKernelTime + _systemStatistics.CpuPercentUserTime;
        double memRatio = 1.0 - ((double)(_systemStatistics.AvailablePhysical) / (double)(_systemStatistics.TotalPhysical));
        double virRatio = 1.0 - ((double)(_systemStatistics.AvailablePageFile) / (double)(_systemStatistics.TotalPageFile));
        
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
            _systemStatistics.CpuPercentKernelTime / 100,
            kernelColour,
            "u",
            _systemStatistics.CpuPercentUserTime / 100,
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
            (_systemStatistics.CpuPercentUserTime / 100).ToString("000.0%"),
            userColour);

        nchars += DrawColumnLabelValue(
            "  Total: ",
            ((double)(_systemStatistics.TotalPhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        nchars += DrawColumnLabelValue(
            "  Total: ",
            ((double)(_systemStatistics.TotalPageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
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
            (_systemStatistics.CpuPercentKernelTime / 100).ToString("000.0%"),
            kernelColour);

        nchars += DrawColumnLabelValue(
            "  Used:  ",
            ((double)(_systemStatistics.TotalPhysical - _systemStatistics.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        nchars += DrawColumnLabelValue(
            "  Used:  ",
            ((double)(_systemStatistics.TotalPageFile - _systemStatistics.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Width - nchars - 4);

        nlines++;
        nchars = 4 + 1 + MetreWidth + 1;

        Terminal.WriteEmptyLineTo(nchars);

        nlines++;
        
        nchars += DrawColumnLabelValue(
            "  Idle:    ",
            (_systemStatistics.CpuPercentIdleTime / 100).ToString("000.0%"),
            ForegroundColour);

        nchars += DrawColumnLabelValue(
            "  Free:  ",
            ((double)(_systemStatistics.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        nchars += DrawColumnLabelValue(
            "  Free:  ",
            ((double)(_systemStatistics.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB"),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Width - nchars);
        
        nlines++;
        
        nchars = DrawColumnLabelValue(
            "Processes: ",
            _systemStatistics.ProcessCount.ToString(),
            ForegroundColour);
#if DEBUG        
        nchars += DrawColumnLabelValue(
            ", Ghosts: ",
            _systemStatistics.GhostProcessCount.ToString(),
            ForegroundColour);
#endif
        nchars += DrawColumnLabelValue(
            ", Threads: ",
            _systemStatistics.ThreadCount.ToString(),
            ForegroundColour);
        
        Terminal.WriteEmptyLineTo(Width - nchars);
        
        Height = nlines;
    }

    protected override void OnLoad()
    {
        _processor.ProcessorUpdated += OnProcessorUpdated;
    }
    
    protected override void OnUnload()
    {
        _processor.ProcessorUpdated -= OnProcessorUpdated;
    }

    private void OnProcessorUpdated(object? sender, ProcessorEventArgs e)
    {
        _systemStatistics = e.SystemStatistics;
        
        Draw();
    }
}
