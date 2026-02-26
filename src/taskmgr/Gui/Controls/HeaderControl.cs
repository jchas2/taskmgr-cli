using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using IProcessor = Task.Manager.Process.IProcessor;
using ProcessorEventArgs = Task.Manager.Process.ProcessorEventArgs;

namespace Task.Manager.Gui.Controls;

public sealed class HeaderControl : Control
{
    private readonly IProcessor processor;
    private readonly AppConfig appConfig;
    private SystemStatistics systemStatistics = new();

    private readonly MetreControl cpuMetre;
    private readonly MetreControl memoryMetre;
    private readonly MetreControl virtualMemoryMetre;
    private readonly MetreControl diskMetre;
    private readonly MetreControl gpuMetre;
    private readonly MetreControl gpuMemMetre;
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
        AppConfig appConfig) 
        : base(terminal)
    {
        this.processor = processor;
        this.appConfig = appConfig;
        
        cpuMetre = new MetreControl(terminal) {
            Text = "Cpu    ",
            DrawStacked = true,
        };

        memoryMetre = new MetreControl(terminal) {
            Text = "Mem    ",
            DrawStacked = false,
        };

        virtualMemoryMetre = new MetreControl(terminal) {
#if __WIN32__            
            Text = "Vir    ",
#endif
#if __APPLE__            
            Text = "Swp    ",
#endif
            DrawStacked = false,
        };

        diskMetre = new MetreControl(terminal) {
            Text = "Dsk    ",
            DrawStacked = false,
        };

        gpuMetre = new MetreControl(terminal) {
            Text = "Gpu    ",
            DrawStacked = false,
        };

        gpuMemMetre = new MetreControl(terminal) {
            Text = "Gpu Mem",
            DrawStacked = false,
        };
        
        statisticsView = new ListView(terminal) {
            EnableScroll = false,
            EnableRowSelect = false,
            ShowColumnHeaders = false,
            Visible = true
        };

        for (int i = 0; i < StatisticsViewColumnCount; i++) {
            statisticsView.ColumnHeaders.Add(new ListViewColumnHeader(string.Empty));
        }

        Controls
            .Add(cpuMetre)
            .Add(memoryMetre)
            .Add(virtualMemoryMetre)
            .Add(diskMetre)
            .Add(gpuMetre)
            .Add(gpuMemMetre)
            .Add(statisticsView);
    }
    
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

        BackgroundColour = appConfig.DefaultTheme.Background;
        ForegroundColour = appConfig.DefaultTheme.Foreground;
        
        Terminal.SetCursorPosition(X, Y);
        Terminal.BackgroundColor = appConfig.DefaultTheme.MenubarBackground;
        Terminal.ForegroundColor = appConfig.DefaultTheme.MenubarForeground;

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

        string cpuInfo = $"{systemStatistics.CpuName} (Cores {systemStatistics.CpuCores})";
        if (appConfig.UseIrixReporting) {
            cpuInfo += " Irix Mode";
        }
        
        Terminal.Write(cpuInfo);
        nchars = cpuInfo.Length + 1;

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
        
        double mbps = systemStatistics.DiskUsage.ToMbpsFromBytes();

        if (mbps > maxMbps) {
            maxMbps = mbps;
        }

        double mbpsRatio = maxMbps > 0
            ? mbps / maxMbps
            : 0;

        double gpuCpu = systemStatistics.GpuPercentTime;
        double gpuMemRatio = 0.0;

        if (systemStatistics.TotalGpuMemory > 0) {
            gpuMemRatio = 1.0 - ((double)(systemStatistics.AvailableGpuMemory) / (double)(systemStatistics.TotalGpuMemory));
        }
        
        ConsoleColor userColour = totalCpu < 0.25 ? appConfig.DefaultTheme.RangeLowBackground
            : totalCpu < 0.75 ? appConfig.DefaultTheme.RangeMidBackground
            : appConfig.DefaultTheme.RangeHighBackground;

        // Switch the kernel colours around to provide some contrast to the User Cpu %.
        ConsoleColor kernelColour = totalCpu < 0.25 ? appConfig.DefaultTheme.RangeMidBackground
            : totalCpu < 0.75 ? appConfig.DefaultTheme.RangeLowBackground
            : appConfig.DefaultTheme.RangeMidBackground;

        ConsoleColor memColour = memRatio < 0.25 ? appConfig.DefaultTheme.RangeLowBackground
            : memRatio < 0.75 ? appConfig.DefaultTheme.RangeMidBackground
            : appConfig.DefaultTheme.RangeHighBackground;

        ConsoleColor virColour = virRatio < 0.25 ? appConfig.DefaultTheme.RangeLowBackground
            : virRatio < 0.75 ? appConfig.DefaultTheme.RangeMidBackground
            : appConfig.DefaultTheme.RangeHighBackground;
        
        ConsoleColor mbpsColour = mbps < 10.0 ? appConfig.DefaultTheme.RangeLowBackground 
            : mbps < 100.0 ? appConfig.DefaultTheme.RangeMidBackground
            : appConfig.DefaultTheme.RangeHighBackground;

        ConsoleColor gpuCpuColour = gpuCpu < 0.25 ? appConfig.DefaultTheme.RangeLowBackground
            : gpuCpu < 0.75 ? appConfig.DefaultTheme.RangeMidBackground
            : appConfig.DefaultTheme.RangeHighBackground;

        ConsoleColor gpuMemColour = gpuMemRatio < 0.25 ? appConfig.DefaultTheme.RangeLowBackground
            : gpuMemRatio < 0.75 ? appConfig.DefaultTheme.RangeMidBackground
            : appConfig.DefaultTheme.RangeHighBackground;
        
        cpuMetre.PercentageSeries1 = systemStatistics.CpuPercentKernelTime;
        cpuMetre.PercentageSeries2 = systemStatistics.CpuPercentUserTime;
        cpuMetre.ColourSeries1 = kernelColour;
        cpuMetre.ColourSeries2 = userColour;
        cpuMetre.LabelSeries1 = string.Empty;
        cpuMetre.LabelSeries2 = appConfig.ShowMetreCpuNumerically
            ? cpuMetre.LabelSeries2 = totalCpu.ToString("000.0%")
            : string.Empty;

        cpuMetre.Draw();

        memoryMetre.PercentageSeries1 = memRatio;
        memoryMetre.ColourSeries1 = memColour;
        memoryMetre.LabelSeries1 = appConfig.ShowMetreMemoryNumerically
            ? (systemStatistics.TotalPhysical - systemStatistics.AvailablePhysical).ToFormattedByteSize() + "/" +
               systemStatistics.TotalPhysical.ToFormattedByteSize()
            : string.Empty;
        
        memoryMetre.Draw();

        virtualMemoryMetre.PercentageSeries1 = virRatio;
        virtualMemoryMetre.ColourSeries1 = virColour;
        virtualMemoryMetre.LabelSeries1 = appConfig.ShowMetreSwapNumerically
            ? (systemStatistics.TotalPageFile - systemStatistics.AvailablePageFile).ToFormattedByteSize() + "/" +
               systemStatistics.TotalPageFile.ToFormattedByteSize()
            : string.Empty;
        
        virtualMemoryMetre.Draw();

        diskMetre.PercentageSeries1 = mbpsRatio;
        diskMetre.ColourSeries1 = mbpsColour;
        diskMetre.LabelSeries1 = appConfig.ShowMetreDiskNumerically
            ? $"{mbps} MB/s"
            : string.Empty;
        
        diskMetre.Draw();
        
        gpuMetre.PercentageSeries1 = gpuCpu;
        gpuMetre.ColourSeries1 = gpuCpuColour;
        gpuMetre.LabelSeries1 = appConfig.ShowMetreCpuNumerically
            ? gpuMetre.LabelSeries1 = gpuCpu.ToString("000.0%")
            : string.Empty;
        
        gpuMetre.Draw();
        
        gpuMemMetre.PercentageSeries1 = gpuMemRatio;
        gpuMemMetre.ColourSeries1 = gpuMemColour;
        gpuMemMetre.LabelSeries1 = appConfig.ShowMetreMemoryNumerically
            ? (systemStatistics.TotalGpuMemory - systemStatistics.AvailableGpuMemory).ToFormattedByteSize() + "/" +
              systemStatistics.TotalGpuMemory.ToFormattedByteSize()
            : string.Empty;
        
        gpuMemMetre.Draw();

        if (statisticsView.Items.Count == 0) {
            return;
        }

        for (int i = 0; i < statisticsView.Items.Count; i++) {
            statisticsView.Items[i].BackgroundColour = BackgroundColour;
            statisticsView.Items[i].ForegroundColour = ForegroundColour;

            for (int j = 0; j < statisticsView.Items[i].SubItems.Count(); j++) {
                statisticsView.Items[i].SubItems[j].BackgroundColor = BackgroundColour;
                statisticsView.Items[i].SubItems[j].ForegroundColor = ForegroundColour;
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
        statisticsView.Items[1].SubItems[3].Text =
            ((double)(systemStatistics.TotalPhysical) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[1].SubItems[3].ForegroundColor = ForegroundColour;
        statisticsView.Items[1].SubItems[5].Text =
            ((double)(systemStatistics.TotalPageFile) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[1].SubItems[5].ForegroundColor = ForegroundColour;
        statisticsView.Items[1].SubItems[7].Text = string.Format("{0,5:####0.0} MB/s", maxMbps);
        statisticsView.Items[1].SubItems[7].ForegroundColor = mbpsColour;

        statisticsView.Items[2].SubItems[1].Text = systemStatistics.CpuPercentKernelTime.ToString("000.0%");
        statisticsView.Items[2].SubItems[1].ForegroundColor = kernelColour;
        statisticsView.Items[2].SubItems[3].Text =
            ((double)(systemStatistics.TotalPhysical - systemStatistics.AvailablePhysical) / 1024 / 1024 / 1024)
            .ToString("0000.0GB");
        statisticsView.Items[2].SubItems[3].ForegroundColor = ForegroundColour;
        statisticsView.Items[2].SubItems[5].Text =
            ((double)(systemStatistics.TotalPageFile - systemStatistics.AvailablePageFile) / 1024 / 1024 / 1024)
            .ToString("0000.0GB");
        statisticsView.Items[2].SubItems[5].ForegroundColor = ForegroundColour;
        statisticsView.Items[2].SubItems[7].Text = string.Empty;
        statisticsView.Items[2].SubItems[7].ForegroundColor = ForegroundColour;

        statisticsView.Items[3].SubItems[1].Text = systemStatistics.CpuPercentIdleTime.ToString("000.0%");
        statisticsView.Items[3].SubItems[1].ForegroundColor = ForegroundColour;
        statisticsView.Items[3].SubItems[3].Text =
            ((double)(systemStatistics.AvailablePhysical) / 1024 / 1024 / 1024).ToString("0000.0GB");
        statisticsView.Items[3].SubItems[3].ForegroundColor = ForegroundColour;
        statisticsView.Items[3].SubItems[5].Text =
            ((double)(systemStatistics.AvailablePageFile) / 1024 / 1024 / 1024).ToString("0000.0GB");
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
        
        MetreControl[] metres = [cpuMetre, memoryMetre, virtualMemoryMetre, diskMetre, gpuMetre, gpuMemMetre ];

        foreach (MetreControl metreControl in metres) {
            metreControl.BackgroundColour = appConfig.DefaultTheme.Background;
            metreControl.ForegroundColour = appConfig.DefaultTheme.Foreground;
            metreControl.MetreStyle = appConfig.MetreStyle;
        }
        
        statisticsView.BackgroundColour = BackgroundColour;
        statisticsView.ForegroundColour = ForegroundColour;
        statisticsView.BackgroundHighlightColour = appConfig.DefaultTheme.BackgroundHighlight;
        statisticsView.ForegroundHighlightColour = appConfig.DefaultTheme.ForegroundHighlight;
        statisticsView.HeaderBackgroundColour = appConfig.DefaultTheme.HeaderBackground;
        statisticsView.HeaderForegroundColour = appConfig.DefaultTheme.HeaderForeground;

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

        statisticsView.Items.Clear();
        
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

        gpuMetre.X = X;
        gpuMetre.Y = Y + 7;
        gpuMetre.Width = MetreWidth;
        gpuMetre.Height = 1;
        gpuMetre.Resize();

        gpuMemMetre.X = X;
        gpuMemMetre.Y = Y + 8;
        gpuMemMetre.Width = MetreWidth;
        gpuMemMetre.Height = 1;
        gpuMemMetre.Resize();
        
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
        base.OnUnload();
        
        processor.ProcessorUpdated -= OnProcessorUpdated;
    }
}