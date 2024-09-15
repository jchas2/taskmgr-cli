using Task.Manager.System.Configuration;

namespace Task.Manager.System.UnitTests;

public class When_Using_ConfigParser
{
    [Fact]
    public void Should_Parse_Min_Config_File()
    {
        var configParser = new ConfigParser(MinConfigFile);
        bool result = configParser.Parse();
        
        Assert.True(result);
        Assert.True(configParser.Sections.Count == 1);
        Assert.Equal("section1", configParser.Sections[0].Name);
        Assert.True(configParser.Sections[0].Contains("key1"));
        Assert.Equal("value1", configParser.Sections[0].GetString("key1"));
    }

    private static string MinConfigFile =>
        @"
# Min config file.
[section1]
key1=value1
";
    
    private static string TestConfigFile =>
        $@"
#####################################################
# Example Config file.
#####################################################

[sort]
key=cpu

[ui]
display=monochrome       ; colour, monochrome
";
}
