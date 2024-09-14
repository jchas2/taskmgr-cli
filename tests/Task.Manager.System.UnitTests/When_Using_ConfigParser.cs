using Task.Manager.System.Configuration;

namespace Task.Manager.System.UnitTests;

public class When_Using_ConfigParser
{
    [Fact]
    public void Should_Parse_File()
    {
        var configParser = new ConfigParser(TestConfigFile);
        bool result = configParser.Parse();
        
        Assert.True(result);
        //Assert.True(configParser.Sections.Count == 2);
        //Assert.Equal("sort", configParser.Sections[0].Name);
        //Assert.Equal("ui", configParser.Sections[1].Name);
    }

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
