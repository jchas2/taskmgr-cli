namespace Task.Manager.Cli.Utils;

public static class StringExtensions
{
    public static string CentreWithLength(this string str, int length)
    {
        if (str.Length >= length) {
            return str.Substring(0, length);
        }
        
        int padding = length - str.Length;
        int padLeft = padding / 2;
        int padRight = padding - padLeft;
        
        return new string(' ', padLeft) + str + new string(' ', padRight);
    }
}
