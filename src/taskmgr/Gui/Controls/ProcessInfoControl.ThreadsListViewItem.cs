using System.Diagnostics;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ProcessInfoControl
{
    private class ThreadListViewItem : ListViewItem
    {
        private string? lastThreadState;
        private string? lastReason;
        private int lastPriority;
        private TimeSpan lastCpuKernelTime;
        private TimeSpan lastCpuUserTime;
        private TimeSpan lastCpuTotalTime;
        
        public ThreadListViewItem(ThreadInfo threadInfo, Theme theme)
            : base(threadInfo.ThreadId.ToString())
        {
            ArgumentNullException.ThrowIfNull(threadInfo);
            
            Theme = theme ?? throw new ArgumentNullException(nameof(theme));
            ThreadId = threadInfo.ThreadId;
            
            SubItems.AddRange(
                new ListViewSubItem(this, threadInfo.ThreadState),
                new ListViewSubItem(this, threadInfo.Reason),
                new ListViewSubItem(this, $"{threadInfo.Priority}"),
                new ListViewSubItem(this, threadInfo.StartAddress.ToHexadecimal()),
                new ListViewSubItem(this, threadInfo.CpuKernelTime.ToString()),
                new ListViewSubItem(this, threadInfo.CpuUserTime.ToString()),
                new ListViewSubItem(this, threadInfo.CpuTotalTime.ToString()));

            UpdateItem(threadInfo);
        }
        
        private Theme Theme { get; }
        
        public int ThreadId { get; private set; }

        private void UpdateSubItem(ListViewSubItem subItem, string text, Func<bool> changeCondition)
        {
            subItem.Text = text;
            
            if (changeCondition.Invoke()) {
                subItem.ForegroundColor = Theme.RangeMidForeground;
            }
        }
        
        public void UpdateItem(ThreadInfo threadInfo)
        {
            ArgumentNullException.ThrowIfNull(threadInfo);
            Debug.Assert(threadInfo.ThreadId == ThreadId);

            foreach (ListViewSubItem subItem in SubItems) {
                subItem.BackgroundColor = Theme.Background;
                subItem.ForegroundColor = Theme.Foreground;
            }
            
            UpdateSubItem(
                SubItems[(int)ThreadColumns.State],
                threadInfo.ThreadState, 
                () => !threadInfo.ThreadState.Equals(lastThreadState));
            
            lastThreadState = threadInfo.ThreadState;
            
            UpdateSubItem(
                SubItems[(int)ThreadColumns.Reason],
                threadInfo.Reason,
                () => !threadInfo.Reason.Equals(lastReason));
            
            lastReason = threadInfo.Reason;
            
            UpdateSubItem(
                SubItems[(int)ThreadColumns.Priority],
                $"{threadInfo.Priority}",
                () => threadInfo.Priority != lastPriority);
            
            lastPriority = threadInfo.Priority;

            UpdateSubItem(
                SubItems[(int)ThreadColumns.CpuKernelTime],
                threadInfo.CpuKernelTime.ToString(),
                () => threadInfo.CpuKernelTime != lastCpuKernelTime);
            
            lastCpuKernelTime = threadInfo.CpuKernelTime;

            UpdateSubItem(
                SubItems[(int)ThreadColumns.CpuUserTime],
                threadInfo.CpuUserTime.ToString(),
                () => threadInfo.CpuUserTime != lastCpuUserTime);
            
            lastCpuUserTime = threadInfo.CpuUserTime;

            UpdateSubItem(
                SubItems[(int)ThreadColumns.CpuTotalTime],
                threadInfo.CpuTotalTime.ToString(),
                () => threadInfo.CpuTotalTime != lastCpuTotalTime);
            
            lastCpuTotalTime = threadInfo.CpuTotalTime;
        }
    }
}
