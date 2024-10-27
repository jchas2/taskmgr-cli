using Moq;
using Task.Manager.Gui.Controls;
using Task.Manager.System;

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
        var terminal = new Mock<ISystemTerminal>();
        var listview = new ListView(terminal.Object);
        listview.Items.AddRange(GetListViewItemData().ToArray());
        
        Assert.True(listview.Items.Count == 3);
        Assert.True(listview.Items[0].Text == "Item 1");
        Assert.True(listview.Items[1].Text == "Item 2");
        Assert.True(listview.Items[2].Text == "Item 3");
    }
}
