using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;
using IProcessor = Task.Manager.Process.IProcessor;
using ProcessorEventArgs = Task.Manager.Process.ProcessorEventArgs;

namespace Task.Manager.Gui.Controls;

public sealed class HeaderControl : Control
{
    private readonly IProcessor processor;
    private readonly Config config;
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
        Theme theme,
        Config config) : base(terminal)
    {
        this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
        this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
        this.config = config ?? throw new ArgumentNullException(nameof(config));
                
        ConfigSection uxSection = config.GetConfigSection(Configuration.Constants.Sections.UX);
        MetreControlStyle metreStyle = uxSection.Contains(Configuration.Constants.Keys.MetreStyle) 
            ? uxSection.GetEnum(Configuration.Constants.Keys.MetreStyle, MetreControlStyle.Dot) 
            : MetreControlStyle.Dot;
        
        BackgroundColour = theme.Background;
        ForegroundColour = theme.Foreground;
        MenubarBackColour = theme.MenubarBackground;
        MenubarForeColour = theme.MenubarForeground;

        cpuMetre = new MetreControl(terminal) {
            Text = "Cpu",
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            MetreStyle = metreStyle,
            DrawStacked = true,
        };

        memoryMetre = new MetreControl(terminal) {
            Text = "Mem",
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            MetreStyle = metreStyle,
            DrawStacked = false,
        };

        virtualMemoryMetre = new MetreControl(terminal) {
#if __WIN32__            
            Text = "Vir",
#endif
#if __APPLE__            
            Text = "Swp",
#endif
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            MetreStyle = metreStyle,
            DrawStacked = false,
        };

        diskMetre = new MetreControl(terminal) {
            Text = "Dsk",
            BackgroundColour = theme.Background,
            ForegroundColour = theme.Foreground,
            MetreStyle = metreStyle,
            DrawStacked = false,
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
    
    public ConsoleColor MenubarBackColour { get; set; } = ConsoleColor.DarkBlue;
    public ConsoleColor MenubarForeColour { get; set; } = ConsoleColor.Gray;

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
        Terminal.BackgroundColor = MenubarBackColour;
        Terminal.ForegroundColor = MenubarForeColour;

        string menubar = "TASK MANAGER";
        int offsetX = Terminal.WindowWidth / 2 - menubar.Length / 2;
        
        Terminal.WriteEmptyLineTo(offsetX);
        Terminal.Write(menubar.ToBold());
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
        
        double totalCpu = systemStatistics.CpuPercentKernelTime + systemStatistics.CpuPercentUserTime;

        double memRatio = 0.0;
        if (systemStatistics.TotalPhysical > 0) {
            memRatio = 1.0 - ((double)(systemStatistics.AvailablePhysical) / (double)(systemStatistics.TotalPhysical));    
        }
        
        double virRatio = 0.0;
        if (systemStatistics.TotalPageFile > 0) {
            virRatio = 1.0 - ((double)(systemStatistics.AvailablePageFile) / (double)(systemStatistics.TotalPageFile));    
        }

        var userColour = totalCpu < 0.25 ? theme.RangeLowBackground
            : totalCpu < 0.75 ? theme.RangeMidBackground
            : theme.RangeHighBackground;

        // Switch the kernel colours around to provide some contrast to the User Cpu %.
        var kernelColour = totalCpu < 0.25 ? theme.RangeMidBackground
            : totalCpu < 0.75 ? theme.RangeLowBackground
            : theme.RangeMidBackground;

        var memColour = memRatio < 0.25 ? theme.RangeLowBackground
            : memRatio < 0.75 ? theme.RangeMidBackground
            : theme.RangeHighBackground;

        var virColour = virRatio < 0.25 ? theme.RangeLowBackground
            : virRatio < 0.75 ? theme.RangeMidBackground
            : theme.RangeHighBackground;
        
        double mbps = systemStatistics.DiskUsage.ToMbpsFromBytes();

        if (mbps > maxMbps) {
            maxMbps = mbps;
        }

        double mbpsRatio = maxMbps > 0
            ? mbps / maxMbps
            : 0;

        ConsoleColor mbpsColour = mbps < 10.0 ? theme.RangeLowBackground 
            : mbps < 100.0 ? theme.RangeMidBackground
            : theme.RangeHighBackground;

        cpuMetre.PercentageSeries1 = systemStatistics.CpuPercentKernelTime;
        cpuMetre.PercentageSeries2 = systemStatistics.CpuPercentUserTime;
        cpuMetre.ColourSeries1 = kernelColour;
        cpuMetre.ColourSeries2 = userColour;
        cpuMetre.LabelSeries1 = string.Empty;
        cpuMetre.LabelSeries2 = totalCpu.ToString("000.0%"); 
        cpuMetre.Draw();
        
        memoryMetre.PercentageSeries1 = memRatio;
        memoryMetre.ColourSeries1 = memColour;
        memoryMetre.LabelSeries1 =
            (systemStatistics.TotalPhysical - systemStatistics.AvailablePhysical).ToFormattedByteSize() + "/" +
             systemStatistics.TotalPhysical.ToFormattedByteSize();
        memoryMetre.Draw();

        virtualMemoryMetre.PercentageSeries1 = virRatio;
        virtualMemoryMetre.ColourSeries1 = virColour;
        virtualMemoryMetre.LabelSeries1 =
            (systemStatistics.TotalPageFile - systemStatistics.AvailablePageFile).ToFormattedByteSize() + "/" +
             systemStatistics.TotalPageFile.ToFormattedByteSize();
        virtualMemoryMetre.Draw();

        diskMetre.PercentageSeries1 = mbpsRatio;
        diskMetre.ColourSeries1 = mbpsColour;
        diskMetre.LabelSeries1 = $"{mbps} MB/s";
        diskMetre.Draw();

        for (int i = 0; i < statisticsView.Items.Count; i++) {
            statisticsView.Items[i].BackgroundColour = theme.Background;
            statisticsView.Items[i].ForegroundColour = theme.Foreground;

            for (int j = 0; j < statisticsView.Items[i].SubItems.Count(); j++) {
                statisticsView.Items[i].SubItems[j].BackgroundColor = theme.Background;
                statisticsView.Items[i].SubItems[j].ForegroundColor = theme.Foreground;
            } 
        }

        statisticsView.Items[0].SubItems[1].Text = totalCpu.ToString("000.0%");
        statisticsView.Items[0].SubItems[1].ForegroundColor = userColour;
        statisticsView.Items[0].SubItems[3].Text = memRatio.ToString("000.0%");
        statisticsView.Items[0].SubItems[3].ForegroundColor = memColour;
        statisticsView.Items[0].SubItems[5].Text = virRatio.ToString("000.0%");
        statisticsView.Items[0].SubItems[5].ForegroundColor = virColour;
        statisticsView.Items[0].SubItems[7].Text = string.Format("{0,5:####0.0} MB/s", mbps);
        statisticsView.Items[0].SubItems[7].ForegroundColor = mbpsColour;

        statisticsView.Items[1].SubItems[1].Text = systemStatistics.CpuPercentUserTime.ToString("000.0%");
        statisticsView.Items[1].SubItems[1].ForegroundColor = userColour;
        statisticsView.Items[1].SubItems[3].Text = ((double)(systemStatistics.TotalPhysical) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[1].SubItems[3].ForegroundColor = ForegroundColour;
        statisticsView.Items[1].SubItems[5].Text = ((double)(systemStatistics.TotalPageFile) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[1].SubItems[5].ForegroundColor = ForegroundColour;
        statisticsView.Items[1].SubItems[7].Text = string.Format("{0,5:####0.0} MB/s", maxMbps);
        statisticsView.Items[1].SubItems[7].ForegroundColor = mbpsColour;

        statisticsView.Items[2].SubItems[1].Text = systemStatistics.CpuPercentKernelTime.ToString("000.0%");
        statisticsView.Items[2].SubItems[1].ForegroundColor = kernelColour;
        statisticsView.Items[2].SubItems[3].Text = ((double)(systemStatistics.TotalPhysical - systemStatistics.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[2].SubItems[3].ForegroundColor = ForegroundColour;
        statisticsView.Items[2].SubItems[5].Text = ((double)(systemStatistics.TotalPageFile - systemStatistics.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[2].SubItems[5].ForegroundColor = ForegroundColour;
        statisticsView.Items[2].SubItems[7].Text = string.Empty;
        statisticsView.Items[2].SubItems[7].ForegroundColor = ForegroundColour;
        
        statisticsView.Items[3].SubItems[1].Text = systemStatistics.CpuPercentIdleTime.ToString("000.0%");
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
        statisticsView.Items[4].SubItems[5].Text = string.Empty;
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

        if (statisticsView.Items.Count == 0) {
#if __WIN32__
            statisticsView.Items.Add(new(["Cpu:   ", "", "Mem:    ", "", "Vir:   ", "", "Disk:", ""]));
#endif
#if __APPLE__
            statisticsView.Items.Add(new(["Cpu:   ", "", "Mem:    ", "", "Swap:  ", "", "Disk:", ""]));
#endif
            statisticsView.Items.Add(new(["User:  ", "", "Total:  ", "", "Total: ", "", "Peak:", ""]));
            statisticsView.Items.Add(new(["Kernel:", "", "Used:   ", "", "Used:  ", "", "", ""]));
            statisticsView.Items.Add(new(["Idle:  ", "", "Free:   ", "", "Free:  ", "", "", ""]));
#if DEBUG
            statisticsView.Items.Add(new(["Proc:  ", "", "Threads:", "", "Ghosts:", "", "", ""]));
#else
            statisticsView.Items.Add(new(["Procs  :",       "", "Threads:",      "", "",         "", "",      ""]));
#endif
        }

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
        statisticsView.Height = statisticsView.Items.Count + 1;
        statisticsView.Resize();
    }

    private void OnProcessorUpdated(object? sender, ProcessorEventArgs e)
    {
        systemStatistics = e.SystemStatistics;
        Draw();
    }
    
    protected override void OnUnload()
    {
        processor.ProcessorUpdated -= OnProcessorUpdated;

        statisticsView.Items.Clear();
        
        foreach (Control control in Controls) {
            control.Unload();
        }
    }
}