namespace Task.Manager.Cli.Utils;

public static class AnsiConsoleStringExtensions
{
    public static string ToRed(this string str) => $"\u001b[31m{str}\u001b[0m";
    public static string ToBlue(this string str) => $"\u001b[34m{str}\u001b[0m";
}