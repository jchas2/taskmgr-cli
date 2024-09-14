using Task.Manager.System.Configuration;

namespace Task.Manager.System.UnitTests;

public class When_Using_ConfigParser
{
    [Fact]
    public void Should_Parse_File()
    {
        var configParser = new ConfigParser();
        configParser.Parse(TestConfigFile);
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
