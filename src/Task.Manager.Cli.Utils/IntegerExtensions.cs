namespace Task.Manager.Cli.Utils;

public static class IntegerExtensions
{
    public static string ToFormattedByteSize(this int num) => ToFormattedByteSize((long)num);
    
    public static string ToFormattedByteSize(this long num)
    {
        string[] byteFormatters = { "B", "KB", "MB", "GB", "TB" };
        int index = 0;
        double count = num;

        while (count >= 1024 && index < byteFormatters.Length - 1) {
            index++;
            count /= 1024;
        }

        return $"{count:0.#} {byteFormatters[index]}";
    }
}