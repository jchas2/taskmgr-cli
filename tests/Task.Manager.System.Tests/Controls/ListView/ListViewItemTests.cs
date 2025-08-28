using Moq;
using Task.Manager.System;
using Task.Manager.System.Controls.ListView;

namespace Task.Manager.Tests.Controls;

public sealed class ListViewItemTests
{
    private readonly Mock<ISystemTerminal> terminalMock = new();
    
    public static List<ListViewItem> GetListViewItemData()
        => new() {
            new ListViewItem("Item 1"),
            new ListViewItem("Item 2"),
            new ListViewItem("Item 3")
        };

    [Fact]
    public void Should_Add_All_Items()
    {
        var listview = new ListView(terminalMock.Object);
        listview.Items.AddRange(GetListViewItemData().ToArray());
        
        Assert.True(listview.Items.Count == 3);
        Assert.True(listview.Items[0].Text == "Item 1");
        Assert.True(listview.Items[1].Text == "Item 2");
        Assert.True(listview.Items[2].Text == "Item 3");
    }
    
    [Fact]
    public void Should_Enumerate_All_Items()
    {
        var listview = new ListView(terminalMock.Object);
        listview.Items.AddRange(GetListViewItemData().ToArray());

        foreach (var item in listview.Items) {
            Assert.NotNull(item);
        }
    }

    [Fact]
    public void Should_Get_Selected_Item()
    {
        var listview = new ListView(terminalMock.Object);
        listview.Items.AddRange(GetListViewItemData().ToArray());
        
        Assert.NotNull(listview.SelectedItem);
        Assert.True(listview.SelectedItem.Text == "Item 1");
    }
    
    // TODO: Tests for enumerating empty listview
    
    // Tests for calling Draw() etc on empty listview
}

