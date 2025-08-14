using System.Diagnostics;
using System.Globalization;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
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
            ProcessInfo processInfo,
            ref SystemStatistics systemStatistics,
            Theme theme) 
            : base(processInfo.FileDescription ?? string.Empty)
        {
            ArgumentNullException.ThrowIfNull(processInfo);
            
            Theme = theme ?? throw new ArgumentNullException(nameof(theme));
            Pid = processInfo.Pid;

            SubItems.AddRange(
                new ListViewSubItem(this, processInfo.Pid.ToString()),
                new ListViewSubItem(this, processInfo.UserName ?? string.Empty),
                new ListViewSubItem(this, processInfo.BasePriority.ToString()),
                new ListViewSubItem(this, processInfo.CpuTimePercent.ToString(CultureInfo.InvariantCulture)),
                new ListViewSubItem(this, processInfo.ThreadCount.ToString()),
                new ListViewSubItem(this, processInfo.UsedMemory.ToString()),
                new ListViewSubItem(this, processInfo.DiskUsage.ToString()),
                new ListViewSubItem(this, processInfo.CmdLine ?? string.Empty));

            UpdateItem(processInfo, ref systemStatistics);
        }

        public int Pid { get; private set; }

        private Theme Theme { get; }

        private void UpdateSubItem(ListViewSubItem subItem, Func<bool> lowCondition, Func<bool> highCondition)
        {
            if (lowCondition.Invoke()) {
                subItem.ForegroundColor = Theme.RangeLowForeground;
            }
            else if (highCondition.Invoke()) {
                subItem.ForegroundColor = Theme.RangeHighForeground;
            }
        }
        
        public void UpdateItem(ProcessInfo processInfo, ref SystemStatistics systemStatistics)
        {
            ArgumentNullException.ThrowIfNull(processInfo);
            Debug.Assert(processInfo.Pid == Pid);
            
            for (int i = 0; i < (int)Columns.Count; i++) {
                SubItems[i].BackgroundColor = Theme.Background;
                SubItems[i].ForegroundColor = Theme.Foreground;
            }
            
            SubItems[(int)Columns.Process].Text = processInfo.FileDescription ?? string.Empty;
            SubItems[(int)Columns.Pid].Text = processInfo.Pid.ToString();
            SubItems[(int)Columns.User].Text = processInfo.UserName ?? string.Empty;

            if (!SubItems[(int)Columns.User].Text.Equals(Environment.UserName, StringComparison.OrdinalIgnoreCase)) {
                SubItems[(int)Columns.User].ForegroundColor = ConsoleColor.DarkGray;
            }
            
            SubItems[(int)Columns.Priority].Text = processInfo.BasePriority.ToString();

            UpdateSubItem(
                SubItems[(int)Columns.Priority], 
                () => processInfo.BasePriority < lastBasePriority,
                () => processInfo.BasePriority > lastBasePriority);
            
            SubItems[(int)Columns.Cpu].Text = (processInfo.CpuTimePercent / 100).ToString("00.00%", CultureInfo.InvariantCulture);

            if (processInfo.CpuTimePercent > 1.0) {
                if (processInfo.CpuTimePercent < 10.0) {
                    SubItems[(int)Columns.Process].ForegroundColor = Theme.RangeLowBackground;
                    SubItems[(int)Columns.Cpu].ForegroundColor = Theme.ForegroundHighlight;
                    SubItems[(int)Columns.Cpu].BackgroundColor = Theme.RangeLowBackground;
                }
                else if (processInfo.CpuTimePercent < 50.0) {
                    SubItems[(int)Columns.Process].ForegroundColor = Theme.RangeMidBackground;
                    SubItems[(int)Columns.Cpu].ForegroundColor = Theme.ForegroundHighlight;
                    SubItems[(int)Columns.Cpu].BackgroundColor = Theme.RangeMidBackground;
                }
                else {
                    SubItems[(int)Columns.Process].ForegroundColor = Theme.RangeHighBackground;
                    SubItems[(int)Columns.Cpu].ForegroundColor = Theme.ForegroundHighlight;
                    SubItems[(int)Columns.Cpu].BackgroundColor = Theme.RangeHighBackground;
                }
            }
            else {
                UpdateSubItem(
                    SubItems[(int)Columns.Cpu], 
                    () => processInfo.CpuTimePercent < lastCpu,
                    () => processInfo.CpuTimePercent > lastCpu);
            }
            
            SubItems[(int)Columns.Threads].Text = processInfo.ThreadCount.ToString();

            UpdateSubItem(
                SubItems[(int)Columns.Threads], 
                () => processInfo.ThreadCount < lastThreadCount,
                () => processInfo.ThreadCount > lastThreadCount);
            
            SubItems[(int)Columns.Memory].Text = processInfo.UsedMemory.ToFormattedByteSize();

            double memRatio = (double)processInfo.UsedMemory / (double)systemStatistics.TotalPhysical;
            
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
                    () => processInfo.UsedMemory < lastUsedMemory,
                    () => processInfo.UsedMemory > lastUsedMemory);
            }
            
            SubItems[(int)Columns.Disk].Text = processInfo.DiskUsage.ToFormattedMbpsFromBytes();
            double mbps = processInfo.DiskUsage.ToMbpsFromBytes(); 
            
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
                    () => processInfo.DiskUsage < lastDiskUsage,
                    () => processInfo.DiskUsage > lastDiskUsage);
            }

            SubItems[(int)Columns.CommandLine].Text = processInfo.CmdLine ?? string.Empty;
            
            lastCpu = processInfo.CpuTimePercent;
            lastBasePriority = processInfo.BasePriority;
            lastThreadCount = processInfo.ThreadCount;
            lastUsedMemory = processInfo.UsedMemory;
            lastDiskUsage = processInfo.DiskUsage;
        }
    }
}
