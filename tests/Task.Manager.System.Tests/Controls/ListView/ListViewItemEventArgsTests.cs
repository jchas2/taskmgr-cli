using Task.Manager.System.Controls.ListView;

namespace Task.Manager.Tests.Controls;

public sealed class ListViewItemEventArgsTests
{
    [Fact]
    public void ListViewItemEventArgs_Ctor()
    {
        ListViewItem item = new("Test");
        ListViewItemEventArgs args = new(item);
        
        Assert.Equal(item, args.Item);
    }
}