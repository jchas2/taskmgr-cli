using Task.Manager.System.Configuration;

namespace Task.Manager.System.UnitTests;

public class When_Using_ConfigParser
{
    [Fact]
    public void Should_Parse_File()
    {
        string data = $@"
            #####################################################
            # Example Config file.
            #####################################################
            
            [sort]
            default-sort-key=cpu
            
            [display]
            color=monochrome
        ";

        var configParser = new ConfigParser();
        configParser.Parse(data);
    }
}
