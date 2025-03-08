using System.Globalization;
using Task.Manager.Configuration;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ProcessControl
{
    private class ProcessListViewItem : ListViewItem
    {
        public ProcessListViewItem(ref ProcessInfo processInfo, Theme theme)
            : base(processInfo.ExeName ?? string.Empty)
        {
            Theme = theme;
            Pid = processInfo.Pid;

            SubItems.AddRange(
                new ListViewSubItem(this, processInfo.Pid.ToString()),
                new ListViewSubItem(this, processInfo.UserName ?? string.Empty),
                new ListViewSubItem(this, processInfo.BasePriority.ToString()),
                new ListViewSubItem(this, processInfo.CpuTimePercent.ToString(CultureInfo.InvariantCulture)),
                new ListViewSubItem(this, processInfo.ThreadCount.ToString()));

            UpdateItem(ref processInfo);
        }

        public int Pid { get; private set; }

        private Theme Theme { get; }

        public void UpdateItem(ref ProcessInfo processInfo)
        {
            for (int i = 0; i < (int)Columns.Count; i++) {
                SubItems[i].BackgroundColor = Theme.Background;
                SubItems[i].ForegroundColor = Theme.Foreground;
            }
            
            if (processInfo.CpuTimePercent > 0.0) {
                if (processInfo.CpuTimePercent < 10.0) {
                    SubItems[(int)Columns.Process].ForegroundColor = Theme.RangeLowBackground;
                    SubItems[(int)Columns.Cpu].ForegroundColor = Theme.RangeLowForeground;
                    SubItems[(int)Columns.Cpu].BackgroundColor = Theme.RangeLowBackground;
                }
                else if (processInfo.CpuTimePercent < 50.0) {
                    SubItems[(int)Columns.Process].ForegroundColor = Theme.RangeMidBackground;
                    SubItems[(int)Columns.Cpu].ForegroundColor = Theme.RangeMidForeground;
                    SubItems[(int)Columns.Cpu].BackgroundColor = Theme.RangeMidBackground;
                }
                else {
                    SubItems[(int)Columns.Process].ForegroundColor = Theme.RangeHighBackground;
                    SubItems[(int)Columns.Cpu].ForegroundColor = Theme.RangeHighForeground;
                    SubItems[(int)Columns.Cpu].BackgroundColor = Theme.RangeHighBackground;
                }
            }
            
            SubItems[0].Text = processInfo.ExeName ?? string.Empty;
            SubItems[1].Text = processInfo.Pid.ToString();
            SubItems[2].Text = processInfo.UserName ?? string.Empty;
            SubItems[3].Text = processInfo.BasePriority.ToString();
            //SubItems[4].Text = (processInfo.CpuTimePercent / 100).ToString("00.00%", CultureInfo.InvariantCulture);
            SubItems[4].Text = processInfo.CpuTimePercent.ToString(CultureInfo.InvariantCulture);
            SubItems[5].Text = processInfo.ThreadCount.ToString();
        }
    }
}