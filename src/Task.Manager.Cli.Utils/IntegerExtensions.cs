namespace Task.Manager.Cli.Utils;

public static class IntegerExtensions
{
    public static string ToFormattedByteSize(this int num) => ToFormattedByteSize((long)num);
    
    public static string ToFormattedByteSize(this long num)
    {
        string[] byteFormatters = ["B", "KB", "MB", "GB", "TB"];
        int index = 0;
        double count = num;

        while (count >= 1024 && index < byteFormatters.Length - 1) {
            index++;
            count /= 1024;
        }

        return $"{count:0.#} {byteFormatters[index]}";
    }

    public static string ToFormattedMbpsFromBytes(this long num)
    {
        double mbps = ToMbpsFromBytes(num);
        return string.Format("{0,5:####0.0} MB/s", mbps);
    }

    public static string ToHexadecimal(this long num) =>
        ((ulong)num).ToHexadecimal();
    
    public static string ToHexadecimal(this ulong num) =>
        "0x" + num.ToString("X16");
    
    public static double ToMbpsFromBytes(this long num) =>
         Math.Ceiling((double)num / 1000000.0 * 10.0) / 10.0;
}
