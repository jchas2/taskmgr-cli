using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.Process;
using Task.Manager.System;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ProcessControl
{
    private class ProcessListViewItem : ListViewItem
    {
        private int lastThreadCount;
        private long lastBasePriority;
        private long lastUsedMemory;
        private long lastDiskUsage;
        private double lastCpu;
        
        public ProcessListViewItem(
            ProcessorInfo processorInfo,
            ref SystemStatistics systemStatistics,
            Theme theme) 
            : base(processorInfo.FileDescription ?? string.Empty)
        {
            ArgumentNullException.ThrowIfNull(processorInfo);
            
            Theme = theme ?? throw new ArgumentNullException(nameof(theme));
            Pid = processorInfo.Pid;

            SubItems.AddRange(
                new ListViewSubItem(this, processorInfo.Pid.ToString()),
                new ListViewSubItem(this, processorInfo.UserName ?? string.Empty),
                new ListViewSubItem(this, processorInfo.BasePriority.ToString()),
                new ListViewSubItem(this, processorInfo.CpuTimePercent.ToString(CultureInfo.InvariantCulture)),
                new ListViewSubItem(this, processorInfo.ThreadCount.ToString()),
                new ListViewSubItem(this, processorInfo.UsedMemory.ToString()),
                new ListViewSubItem(this, processorInfo.DiskUsage.ToString()),
                new ListViewSubItem(this, processorInfo.CmdLine ?? string.Empty));

            UpdateItem(processorInfo, ref systemStatistics);
        }

        public int Pid { get; private set; }

        private Theme Theme { get; }

        private void UpdateSubItem(ListViewSubItem subItem, Func<bool> condition)
        {
            if (condition.Invoke()) {
                subItem.ForegroundColor = Theme.RangeMidForeground;
            }
        }
        
        public void UpdateItem(ProcessorInfo processorInfo, ref SystemStatistics systemStatistics)
        {
            ArgumentNullException.ThrowIfNull(processorInfo);
            Debug.Assert(processorInfo.Pid == Pid);
            
            for (int i = 0; i < (int)Columns.Count; i++) {
                SubItems[i].BackgroundColor = Theme.Background;
                SubItems[i].ForegroundColor = Theme.Foreground;
            }
            
            SubItems[(int)Columns.Process].Text = processorInfo.FileDescription ?? string.Empty;
            SubItems[(int)Columns.Pid].Text = processorInfo.Pid.ToString();
            SubItems[(int)Columns.User].Text = processorInfo.UserName ?? string.Empty;

            if (!SubItems[(int)Columns.User].Text.Equals(Environment.UserName, StringComparison.OrdinalIgnoreCase)) {
                SubItems[(int)Columns.User].ForegroundColor = ConsoleColor.DarkGray;
            }
            
            SubItems[(int)Columns.Priority].Text = processorInfo.BasePriority.ToString();

            UpdateSubItem(
                SubItems[(int)Columns.Priority],
                () => processorInfo.BasePriority != lastBasePriority);
            
            SubItems[(int)Columns.Cpu].Text = processorInfo.CpuTimePercent.ToString("00.00%", CultureInfo.InvariantCulture);
            bool cpuHighCoreUsage = SystemInfo.GetCpuHighCoreUsage(processorInfo.CpuTimePercent);
            
            if (cpuHighCoreUsage) {
                SubItems[(int)Columns.Process].ForegroundColor = Theme.RangeHighBackground;
                SubItems[(int)Columns.Cpu].ForegroundColor = Theme.ForegroundHighlight;
                SubItems[(int)Columns.Cpu].BackgroundColor = Theme.RangeHighBackground;
            }
            else {
                UpdateSubItem(
                    SubItems[(int)Columns.Cpu],
                    () => processorInfo.CpuTimePercent != lastCpu);
            }
            
            SubItems[(int)Columns.Threads].Text = processorInfo.ThreadCount.ToString();

            UpdateSubItem(
                SubItems[(int)Columns.Threads], 
                () => processorInfo.ThreadCount != lastThreadCount);
            
            SubItems[(int)Columns.Memory].Text = processorInfo.UsedMemory.ToFormattedByteSize();

            double memRatio = (double)processorInfo.UsedMemory / (double)systemStatistics.TotalPhysical;
            
            if (memRatio > 0.1 && memRatio <= 0.2) {
                SubItems[(int)Columns.Memory].ForegroundColor = Theme.ForegroundHighlight;
                SubItems[(int)Columns.Memory].BackgroundColor = Theme.RangeLowBackground;
            }
            else if (memRatio > 0.2 && memRatio <= 0.5) {
                SubItems[(int)Columns.Memory].ForegroundColor = Theme.ForegroundHighlight;
                SubItems[(int)Columns.Memory].BackgroundColor = Theme.RangeMidBackground;
            }
            else if (memRatio > 0.5) {
                SubItems[(int)Columns.Memory].ForegroundColor = Theme.ForegroundHighlight;
                SubItems[(int)Columns.Memory].BackgroundColor = Theme.RangeHighBackground;
            }
            else {
                UpdateSubItem(
                    SubItems[(int)Columns.Memory], 
                    () => processorInfo.UsedMemory != lastUsedMemory);
            }
            
            SubItems[(int)Columns.Disk].Text = processorInfo.DiskUsage.ToFormattedMbpsFromBytes();
            double mbps = processorInfo.DiskUsage.ToMbpsFromBytes(); 
            
            if (mbps > 1.0) {
                if (mbps < 10.0) {
                    SubItems[(int)Columns.Disk].ForegroundColor = Theme.ForegroundHighlight;
                    SubItems[(int)Columns.Disk].BackgroundColor = Theme.RangeLowBackground;
                }
                else if (mbps < 100.0) {
                    SubItems[(int)Columns.Disk].ForegroundColor = Theme.ForegroundHighlight;
                    SubItems[(int)Columns.Disk].BackgroundColor = Theme.RangeMidBackground;
                }
                else {
                    SubItems[(int)Columns.Disk].ForegroundColor = Theme.ForegroundHighlight;
                    SubItems[(int)Columns.Disk].BackgroundColor = Theme.RangeHighBackground;
                }
            }
            else {
                UpdateSubItem( 
                    SubItems[(int)Columns.Disk], 
                    () => processorInfo.DiskUsage != lastDiskUsage);
            }

            if (!processorInfo.IsDaemon) {
                SubItems[(int)Columns.Process].ForegroundColor = Theme.ColumnCommandNormalUserSpace;
                SubItems[(int)Columns.CommandLine].ForegroundColor = Theme.ColumnCommandNormalUserSpace;
            }

            if (processorInfo.IsLowPriority) {
                SubItems[(int)Columns.Process].ForegroundColor = Theme.ColumnCommandLowPriority;
                SubItems[(int)Columns.CommandLine].ForegroundColor = Theme.ColumnCommandLowPriority;
            }

            if (mbps >= 100.0) {
                SubItems[(int)Columns.Process].ForegroundColor = Theme.ColumnCommandIoBound;
                SubItems[(int)Columns.CommandLine].ForegroundColor = Theme.ColumnCommandIoBound;
            }

            if (cpuHighCoreUsage) {
                SubItems[(int)Columns.Process].ForegroundColor = Theme.ColumnCommandHighCpu;
                SubItems[(int)Columns.CommandLine].ForegroundColor = Theme.ColumnCommandHighCpu;
            }            

            lastCpu = processorInfo.CpuTimePercent;
            lastBasePriority = processorInfo.BasePriority;
            lastThreadCount = processorInfo.ThreadCount;
            lastUsedMemory = processorInfo.UsedMemory;
            lastDiskUsage = processorInfo.DiskUsage;
        }
    }
}
