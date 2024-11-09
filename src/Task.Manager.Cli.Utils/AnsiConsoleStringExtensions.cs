namespace Task.Manager.Cli.Utils;

public static class AnsiConsoleStringExtensions
{
    public static string ToAnsiColour(this string str, ConsoleColor colour)
    {
        return colour switch {
            ConsoleColor.Black => str.ToBlack(),
            ConsoleColor.DarkBlue => str.ToDarkBlue(),
            ConsoleColor.DarkGreen => str.ToDarkGreen(),
            ConsoleColor.DarkCyan => str.ToDarkCyan(),
            ConsoleColor.DarkRed => str.ToDarkRed(),
            ConsoleColor.DarkMagenta => str.ToDarkMagenta(),
            ConsoleColor.DarkYellow => str.ToDarkYellow(),
            ConsoleColor.Gray => str.ToGray(),
            ConsoleColor.DarkGray => str.ToDarkGray(),
            ConsoleColor.Blue => str.ToBlue(),
            ConsoleColor.Green => str.ToGreen(),
            ConsoleColor.Cyan => str.ToCyan(),
            ConsoleColor.Red => str.ToRed(),
            ConsoleColor.Magenta => str.ToMagenta(),
            ConsoleColor.Yellow => str.ToYellow(),
            ConsoleColor.White => str.ToWhite(),
            _ => throw new ArgumentOutOfRangeException(nameof(colour), colour, null)
        };
    }
    
    public static string ToBlack(this string str) => $"\u001b[30m{str}\u001b[0m";
    public static string ToBlue(this string str) => $"\u001b[94m{str}\u001b[0m";
    public static string ToCyan(this string str) => $"\u001b[96m{str}\u001b[0m";
    public static string ToDarkBlue(this string str) => $"\u001b[34m{str}\u001b[0m";
    public static string ToDarkCyan(this string str) => $"\u001b[36m{str}\u001b[0m";
    public static string ToDarkGreen(this string str) => $"\u001b[32m{str}\u001b[0m";
    public static string ToDarkGray(this string str) => $"\u001b[90m{str}\u001b[0m";
    public static string ToDarkMagenta(this string str) => $"\u001b[35m{str}\u001b[0m";
    public static string ToDarkRed(this string str) => $"\u001b[31m{str}\u001b[0m";
    public static string ToDarkYellow(this string str) => $"\u001b[33m{str}\u001b[0m";
    public static string ToGray(this string str) => $"\u001b[37m{str}\u001b[0m";
    public static string ToGreen(this string str) => $"\u001b[92m{str}\u001b[0m";
    public static string ToMagenta(this string str) => $"\u001b[95m{str}\u001b[0m";
    public static string ToRed(this string str) => $"\u001b[91m{str}\u001b[0m";
    public static string ToWhite(this string str) => $"\u001b[97m{str}\u001b[0m";
    public static string ToYellow(this string str) => $"\u001b[93m{str}\u001b[0m";
}
