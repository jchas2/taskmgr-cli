namespace Task.Manager.Cli.Utils.Tests;

public class When_Using_ConsoleColorUtils
{
    [Theory]
    [InlineData("Black", ConsoleColor.Black)]
    [InlineData("Blue", ConsoleColor.Blue)]
    [InlineData("Cyan", ConsoleColor.Cyan)]
    [InlineData("Gray", ConsoleColor.Gray)]
    [InlineData("Green", ConsoleColor.Green)]
    [InlineData("Magenta", ConsoleColor.Magenta)]
    [InlineData("Red", ConsoleColor.Red)]
    [InlineData("White", ConsoleColor.White)]
    [InlineData("Yellow", ConsoleColor.Yellow)]
    [InlineData("DarkBlue", ConsoleColor.DarkBlue)]
    [InlineData("DarkCyan", ConsoleColor.DarkCyan)]
    [InlineData("DarkGreen", ConsoleColor.DarkGreen)]
    [InlineData("DarkGray", ConsoleColor.DarkGray)]
    [InlineData("DarkMagenta", ConsoleColor.DarkMagenta)]
    [InlineData("DarkRed", ConsoleColor.DarkRed)]
    [InlineData("DarkYellow", ConsoleColor.DarkYellow)]
    public void Should_Get_Colour_FromName(string name, ConsoleColor expected)
    {
        var colour = ConsoleColorUtils.FromName(
            name, name != "Black" 
                ? ConsoleColor.Black 
                : ConsoleColor.White);
        
        Assert.Equal(expected, colour);
    }
}
