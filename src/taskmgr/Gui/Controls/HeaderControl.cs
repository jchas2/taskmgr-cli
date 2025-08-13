using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public sealed class HeaderControl : Control
{
    private readonly IProcessor processor;
    private readonly Theme theme;
    private SystemStatistics systemStatistics = new();

    private readonly MetreControl cpuMetre;
    private readonly MetreControl memoryMetre;
    private readonly MetreControl virtualMemoryMetre;
    private readonly MetreControl diskMetre;
    private readonly ListView statisticsView;
    
    private double maxMbps = 0;
    
    private const int MetreWidth = 52;
    private const int StatisticsViewColumnCount = 8;
    private const int CpuLabelColumnWidth = 7;
    private const int CpuDataColumnWidth = 7;
    private const int MemoryLabelColumnWidth = 8;
    private const int MemoryDataColumnWidth = 10;
    private const int VirtualMemoryLabelColumnWidth = 7;
    private const int VirtualMemoryDataColumnWidth = 10;
    private const int DiskLabelColumnWidth = 5;
    private const int DiskDataColumnWidth = 14;

    public HeaderControl(
        IProcessor processor, 
        ISystemTerminal terminal, 
        Theme theme) : base(terminal)
    {
        this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
        this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
        
        BackgroundColour = theme.Background;
        ForegroundColour = theme.Foreground;
        MenubarColour = theme.Menubar;

        cpuMetre = new MetreControl(terminal) {
            Text = "Cpu",
            DrawStacked = true,
            LabelSeries1 = "k",
            LabelSeries2 = "u"
        };

        memoryMetre = new MetreControl(terminal) {
            Text = "Mem",
            DrawStacked = false,
            LabelSeries1 = "m"
        };

        virtualMemoryMetre = new MetreControl(terminal) {
            Text = "Vir",
            DrawStacked = false,
            LabelSeries1 = "v"
        };

        diskMetre = new MetreControl(terminal) {
            Text = "Dsk",
            DrawStacked = false,
            LabelSeries1 = string.Empty
        };
        
        statisticsView = new ListView(terminal) {
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            EnableScroll = false,
            EnableRowSelect = false,
            ShowColumnHeaders = false,
            Visible = true
        };

        for (int i = 0; i < StatisticsViewColumnCount; i++) {
            statisticsView.ColumnHeaders.Add(new ListViewColumnHeader(string.Empty));
        }
    }
    
    public ConsoleColor MenubarColour { get; set; } = ConsoleColor.DarkGray;

    protected override void OnDraw()
    {
        try {
            Control.DrawingLockAcquire();
            OnDrawInternal();
        }
        finally {
            Control.DrawingLockRelease();
        }
    }

    private void OnDrawInternal()
    {
        using TerminalColourRestorer _ = new();

        Terminal.SetCursorPosition(X, Y);
        Terminal.BackgroundColor = MenubarColour;
        Terminal.ForegroundColor = ForegroundColour;

        string menubar = "Task Manager CLI";
        int offsetX = Terminal.WindowWidth / 2 - menubar.Length / 2;
        
        Terminal.WriteEmptyLineTo(offsetX);
        Terminal.Write(menubar);
        Terminal.WriteEmptyLineTo(Width - offsetX - menubar.Length);
        
        Terminal.BackgroundColor = BackgroundColour;

        Terminal.Write(
            $"{systemStatistics.MachineName}  ({systemStatistics.OsVersion})  IP {systemStatistics.PrivateIPv4Address}");

        int nchars =
            systemStatistics.MachineName.Length + 3 +
            systemStatistics.OsVersion.Length + 6 +
            systemStatistics.PrivateIPv4Address.Length;
        
        Terminal.WriteEmptyLineTo(Width - nchars);
        
        Terminal.Write(
            $"{systemStatistics.CpuName} (Cores {systemStatistics.CpuCores})");

        nchars =
            systemStatistics.CpuName.Length + 8 +
            systemStatistics.CpuCores.ToString().Length + 1;

        Terminal.WriteEmptyLineTo(Width - nchars);
        Terminal.WriteEmptyLine();
        
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
        
        double mbps = systemStatistics.DiskUsage.ToMbpsFromBytes();

        if (mbps > maxMbps) {
            maxMbps = mbps;
        }

        double mbpsRatio = maxMbps > 0
            ? mbps / maxMbps
            : 0;

        ConsoleColor mbpsColour = mbps < 10.0 ? ConsoleColor.DarkGreen 
            : mbps < 100.0 ? ConsoleColor.DarkYellow
            : ConsoleColor.Red;

        cpuMetre.PercentageSeries1 = systemStatistics.CpuPercentKernelTime / 100;
        cpuMetre.PercentageSeries2 = systemStatistics.CpuPercentUserTime / 100;
        cpuMetre.ColourSeries1 = kernelColour;
        cpuMetre.ColourSeries2 = userColour;
        cpuMetre.Draw();
        
        memoryMetre.PercentageSeries1 = memRatio;
        memoryMetre.ColourSeries1 = memColour;
        memoryMetre.Draw();

        virtualMemoryMetre.PercentageSeries1 = virRatio;
        virtualMemoryMetre.ColourSeries1 = virColour;
        virtualMemoryMetre.Draw();

        diskMetre.PercentageSeries1 = mbpsRatio;
        diskMetre.ColourSeries1 = mbpsColour;
        diskMetre.Draw();

        statisticsView.Items[0].SubItems[1].Text = (totalCpu / 100).ToString("000.0%");
        statisticsView.Items[0].SubItems[1].ForegroundColor = userColour;
        statisticsView.Items[0].SubItems[3].Text = memRatio.ToString("000.0%");
        statisticsView.Items[0].SubItems[3].ForegroundColor = memColour;
        statisticsView.Items[0].SubItems[5].Text = virRatio.ToString("000.0%");
        statisticsView.Items[0].SubItems[5].ForegroundColor = virColour;
        statisticsView.Items[0].SubItems[7].Text = string.Format("{0,5:####0.0} MB/s", mbps);
        statisticsView.Items[0].SubItems[7].ForegroundColor = mbpsColour;

        statisticsView.Items[1].SubItems[1].Text = (systemStatistics.CpuPercentUserTime / 100).ToString("000.0%");
        statisticsView.Items[1].SubItems[1].ForegroundColor = userColour;
        statisticsView.Items[1].SubItems[3].Text = ((double)(systemStatistics.TotalPhysical) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[1].SubItems[3].ForegroundColor = ForegroundColour;
        statisticsView.Items[1].SubItems[5].Text = ((double)(systemStatistics.TotalPageFile) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[1].SubItems[5].ForegroundColor = ForegroundColour;
        statisticsView.Items[1].SubItems[7].Text = string.Format("{0,5:####0.0} MB/s", maxMbps);
        statisticsView.Items[1].SubItems[7].ForegroundColor = mbpsColour;

        statisticsView.Items[2].SubItems[1].Text = (systemStatistics.CpuPercentKernelTime / 100).ToString("000.0%");
        statisticsView.Items[2].SubItems[1].ForegroundColor = kernelColour;
        statisticsView.Items[2].SubItems[3].Text = ((double)(systemStatistics.TotalPhysical - systemStatistics.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[2].SubItems[3].ForegroundColor = ForegroundColour;
        statisticsView.Items[2].SubItems[5].Text = ((double)(systemStatistics.TotalPageFile - systemStatistics.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[2].SubItems[5].ForegroundColor = ForegroundColour;
        statisticsView.Items[2].SubItems[7].Text = string.Empty;
        statisticsView.Items[2].SubItems[7].ForegroundColor = ForegroundColour;
        
        statisticsView.Items[3].SubItems[1].Text = (systemStatistics.CpuPercentIdleTime / 100).ToString("000.0%");
        statisticsView.Items[3].SubItems[1].ForegroundColor = ForegroundColour;
        statisticsView.Items[3].SubItems[3].Text = ((double)(systemStatistics.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[3].SubItems[3].ForegroundColor = ForegroundColour;
        statisticsView.Items[3].SubItems[5].Text = ((double)(systemStatistics.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[3].SubItems[5].ForegroundColor = ForegroundColour;
        statisticsView.Items[3].SubItems[7].Text = string.Empty;
        statisticsView.Items[3].SubItems[7].ForegroundColor = ForegroundColour;

        statisticsView.Items[4].SubItems[1].Text = systemStatistics.ProcessCount.ToString();
        statisticsView.Items[4].SubItems[1].ForegroundColor = ForegroundColour;
        statisticsView.Items[4].SubItems[3].Text = systemStatistics.ThreadCount.ToString();
        statisticsView.Items[4].SubItems[3].ForegroundColor = ForegroundColour;
#if DEBUG        
        statisticsView.Items[4].SubItems[5].Text = systemStatistics.GhostProcessCount.ToString();
#else
        statisticsView.Items[4].SubItems[5].Text = string.Empty;
#endif
        statisticsView.Items[4].SubItems[5].ForegroundColor = ForegroundColour;
        statisticsView.Items[4].SubItems[7].Text = string.Empty;
        statisticsView.Items[4].SubItems[7].ForegroundColor = ForegroundColour;
        
        statisticsView.Draw();
    }

    protected override void OnLoad()
    {
        foreach (Control control in Controls) {
            control.Load();
        }

        int[] columnWidths = [
            CpuLabelColumnWidth,
            CpuDataColumnWidth,
            MemoryLabelColumnWidth,
            MemoryDataColumnWidth,
            VirtualMemoryLabelColumnWidth,
            VirtualMemoryDataColumnWidth,
            DiskLabelColumnWidth,
            DiskDataColumnWidth
        ];

        for (int i = 0; i < columnWidths.Length; i++) {
            statisticsView.ColumnHeaders[i].Width = columnWidths[i];
        }
        
        for (int i = 0; i < statisticsView.ColumnHeaders.Count(); i++) {
            statisticsView.ColumnHeaders[i].RightAligned = (i + 1) % 2 == 0;
        }

        statisticsView.Items.Add(new(["Cpu:   ",       "", "Mem:    ",      "", "Vir:   ",    "", "Disk:", ""]));
        statisticsView.Items.Add(new(["User:  ",       "", "Total:  ",      "", "Total: ",    "", "Peak:", ""]));
        statisticsView.Items.Add(new(["Kernel:",       "", "Used:   ",      "", "Used:  ",    "", "",      ""]));
        statisticsView.Items.Add(new(["Idle:  ",       "", "Free:   ",      "", "Free:  ",    "", "",      ""]));
#if DEBUG        
        statisticsView.Items.Add(new(["Proc:  ",       "", "Threads:",      "", "Ghosts:",    "", "",      ""]));
#else
        statisticsView.Items.Add(new(["Procs  :",       "", "Threads:",      "", "",         "", "",      ""]));
#endif
        
        processor.ProcessorUpdated += OnProcessorUpdated; 
    }

    protected override void OnResize()
    {
        Clear();

        cpuMetre.X = X;
        cpuMetre.Y = Y + 3;
        cpuMetre.Width = MetreWidth;
        cpuMetre.Height = 1;
        cpuMetre.Resize();

        memoryMetre.X = X;
        memoryMetre.Y = Y + 4;
        memoryMetre.Width = MetreWidth;
        memoryMetre.Height = 1;
        memoryMetre.Resize();

        virtualMemoryMetre.X = X;
        virtualMemoryMetre.Y = Y + 5;
        virtualMemoryMetre.Width = MetreWidth;
        virtualMemoryMetre.Height = 1;
        virtualMemoryMetre.Resize();

        diskMetre.X = X;
        diskMetre.Y = Y + 6;
        diskMetre.Width = MetreWidth;
        diskMetre.Height = 1;
        diskMetre.Resize();

        statisticsView.X = X + cpuMetre.Width + 2;
        statisticsView.Y = cpuMetre.Y;
        statisticsView.Width = Width - MetreWidth - 2;
        statisticsView.Height = statisticsView.Items.Count;
        statisticsView.Resize();
    }

    protected override void OnUnload()
    {
        statisticsView.Items.Clear();
        
        foreach (Control control in Controls) {
            control.Unload();
        }

        processor.ProcessorUpdated -= OnProcessorUpdated;
    }

    private void OnProcessorUpdated(object? sender, ProcessorEventArgs e)
    {
        systemStatistics = e.SystemStatistics;
        Draw();
    }
}