using Task.Manager.Configuration;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ThreadsControl
{
    private class ThreadListViewItem : ListViewItem
    {
        public ThreadListViewItem(ThreadInfo threadInfo, Theme theme)
            : base(threadInfo.ThreadId.ToString())
        {
            ArgumentNullException.ThrowIfNull(theme, nameof(theme));
            
            SubItems.AddRange(
                new ListViewSubItem(this, threadInfo.ThreadState),
                new ListViewSubItem(this, threadInfo.Reason),
                new ListViewSubItem(this, $"{threadInfo.Priority}"));
            
            for (int i = 0; i < (int)Columns.Count; i++) {
                SubItems[i].BackgroundColor = theme.Background;
                SubItems[i].ForegroundColor = theme.Foreground;
            }
        }
    }
}