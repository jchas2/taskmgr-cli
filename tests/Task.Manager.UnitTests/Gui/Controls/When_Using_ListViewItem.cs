using Task.Manager.Gui.Controls;

namespace Task.Manager.UnitTests.Gui.Controls;

public sealed class When_Using_ListViewItem
{
    public static List<ListViewItem> GetListViewItemData()
        => new() {
            new ListViewItem("Item 1"),
            new ListViewItem("Item 2"),
            new ListViewItem("Item 3")
        };

    [Fact]
    public void Should_Add_All_Items()
    {
        var listview = new ListView();
        listview.Items.AddRange(GetListViewItemData().ToArray());
        
        Assert.True(listview.Items.Count == 3);
        
    }
}
