namespace Task.Manager.Cli.Utils.Tests;

public sealed class IntegerExtensionsTests
{
    public static TheoryData<long, string> ByteData()
        => new()
        {
            { 1, "1 B" },
            { 16, "16 B" },
            { 32, "32 B" },
            { 64, "64 B" },
            { 264, "264 B" },
            { 512, "512 B" },
            { 1024, "1 KB" },
            { 1536, "1.5 KB" },
            { 1024 * 2, "2 KB" },
            { 1024 * 512, "512 KB" },
            { 1024 * 1024, "1 MB" },
            { 1024 * 1024 * 1024, "1 GB" }
        };
    
    [Theory]
    [MemberData(nameof(ByteData))]
    public void Should_Format_ByteSize(long num, string expected)
    {
        string value = num.ToFormattedByteSize();
        Assert.Equal(expected, value);
    }

}