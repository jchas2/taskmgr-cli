using Moq;
using Task.Manager.System.Controls.ListView;
using ListViewControl = Task.Manager.System.Controls.ListView.ListView;

namespace Task.Manager.System.Tests.Controls.ListView;

public sealed class ListViewSubItemTests
{
    [Fact]
    public void Constructor_With_Text_Initialises_Correctly()
    {
        ListViewItem item = new("Item");
        ListViewSubItem subItem = new(item, "Sub Item");
        
        Assert.Equal("Sub Item", subItem.Text);
        Assert.True(ConsoleColor.Black == subItem.BackgroundColor);
        Assert.True(ConsoleColor.White == subItem.ForegroundColor);
    }
    
    [Fact]
    public void Constructor_With_All_Parameters_Initialises_Correctly()
    {
        ListViewItem item = new("Item");
        
        ListViewSubItem subItem = new(
            item,
            "Sub Item", 
            ConsoleColor.Green, 
            ConsoleColor.Black);
        
        Assert.Equal("Sub Item", subItem.Text);
        Assert.True(ConsoleColor.Green == subItem.BackgroundColor);
        Assert.True(ConsoleColor.Black == subItem.ForegroundColor);
    }
}

