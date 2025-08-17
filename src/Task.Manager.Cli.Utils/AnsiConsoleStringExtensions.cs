namespace Task.Manager.Cli.Utils;

public static class AnsiConsoleStringExtensions
{
    public static string ToBold(this string str)=> $"\u001b[1m{str}\u001b[1m";
    
    public static string ToColour(this string str, ConsoleColor foreground, ConsoleColor background)
    {
        string buffer = string.Empty;

        buffer += background switch {
            ConsoleColor.Black => "\u001b[40m",
            ConsoleColor.DarkBlue => "\u001b[44m",
            ConsoleColor.DarkGreen => "\u001b[42m",
            ConsoleColor.DarkCyan => "\u001b[46m",
            ConsoleColor.DarkRed => "\u001b[41m",
            ConsoleColor.DarkMagenta => "\u001b[45m",
            ConsoleColor.DarkYellow => "\u001b[43m",
            ConsoleColor.Gray => "\u001b[47m",
            ConsoleColor.DarkGray => "\u001b[100m",
            ConsoleColor.Blue => "\u001b[104m",
            ConsoleColor.Green => "\u001b[102m",
            ConsoleColor.Cyan => "\u001b[106m",
            ConsoleColor.Red => "\u001b[101m",
            ConsoleColor.Magenta => "\u001b[105m",
            ConsoleColor.Yellow => "\u001b[103m",
            ConsoleColor.White => "\u001b[107m",
            _ => throw new ArgumentOutOfRangeException(nameof(background), background, null)
        };

        buffer += foreground switch {
            ConsoleColor.Black => "\u001b[30m",
            ConsoleColor.DarkBlue => "\u001b[34m",
            ConsoleColor.DarkGreen => "\u001b[32m",
            ConsoleColor.DarkCyan => "\u001b[36m",
            ConsoleColor.DarkRed => "\u001b[31m",
            ConsoleColor.DarkMagenta => "\u001b[35m",
            ConsoleColor.DarkYellow => "\u001b[33m",
            ConsoleColor.Gray => "\u001b[37m",
            ConsoleColor.DarkGray => "\u001b[90m",
            ConsoleColor.Blue => "\u001b[94m",
            ConsoleColor.Green => "\u001b[92m",
            ConsoleColor.Cyan => "\u001b[96m",
            ConsoleColor.Red => "\u001b[91m",
            ConsoleColor.Magenta => "\u001b[95m",
            ConsoleColor.Yellow => "\u001b[93m",
            ConsoleColor.White => "\u001b[97m",
            _ => throw new ArgumentOutOfRangeException(nameof(background), background, null)
        };

        return buffer + str + "\u001b[0m";
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
