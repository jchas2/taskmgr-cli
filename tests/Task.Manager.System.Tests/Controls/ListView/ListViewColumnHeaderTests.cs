using Task.Manager.System.Controls.ListView;

namespace Task.Manager.System.Tests.Controls.ListView;

public sealed class ListViewColumnHeaderTests
{
    [Fact]
    public void Constructor_With_Text_Initialises_Correctly()
    {
        string headerText = "Test Header";
        var header = new ListViewColumnHeader(headerText);
        
        Assert.Equal(headerText, header.Text);
        Assert.Null(header.BackgroundColour);
        Assert.Null(header.ForegroundColour);
        Assert.False(header.RightAligned);
    }

    [Fact]
    public void Constructor_With_Text_Throws_ArgumentNullException_For_Null_Text()
    {
        string? nullText = null;
        Assert.Throws<ArgumentNullException>(() => new ListViewColumnHeader(nullText!));
    }
    
    [Fact]
    public void Constructor_With_All_Parameters_Initialises_Correctly()
    {
        string headerText = "Full Header";
        ConsoleColor bgColor = ConsoleColor.Blue;
        ConsoleColor fgColor = ConsoleColor.Yellow;
        var header = new ListViewColumnHeader(headerText, bgColor, fgColor);
        
        Assert.Equal(headerText, header.Text);
        Assert.True(bgColor == header.BackgroundColour);
        Assert.True(fgColor == header.ForegroundColour);
        Assert.False(header.RightAligned);
    }
    
    [Fact]
    public void Constructor_With_All_Parameters_Throws_ArgumentNullException_For_Null_Text()
    {
        string? nullText = null;
        ConsoleColor bgColor = ConsoleColor.Blue;
        ConsoleColor fgColor = ConsoleColor.Yellow;

        Assert.Throws<ArgumentNullException>(() => new ListViewColumnHeader(nullText!, bgColor, fgColor));
    }    
}