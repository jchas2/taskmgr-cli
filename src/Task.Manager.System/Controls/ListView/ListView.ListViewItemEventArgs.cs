namespace Task.Manager.System.Controls.ListView;

public sealed class ListViewItemEventArgs(ListViewItem item) : EventArgs
{
    public ListViewItem Item { get; } = item;
}