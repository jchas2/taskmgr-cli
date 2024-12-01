using Task.Manager.System.Configuration;

namespace Task.Manager.System.UnitTests.Configuration;

public sealed class When_Using_ConfigParser
{
    private static string MinConfigFile => @"
[section1]
key1=value1
key2=value2";

    private static string MinConfigFileWithAllDataTypes = @"
[data-types]
string-key=string value
bool-true=true
bool-false=false
int-key1=12345678
int-key2=-12345678
console-color-black=black
console-color-darkblue=darkblue
console-color-darkgreen=darkgreen
console-color-darkcyan=darkcyan
console-color-darkred=darkred
console-color-darkmagenta=darkmagenta
console-color-darkyellow=darkyellow
console-color-gray=gray
console-color-darkgray=darkgray
console-color-blue=blue
console-color-green=green
console-color-cyan=cyan
console-color-red=red
console-color-magenta=magenta
console-color-yellow=yellow
console-color-white=white
";
    
    private static string TestConfigFile => $@"
#####################################################
# Example Config file.
#####################################################

[filter]
pid=-1         ; Process Id to filter on. Must be > 0.
username=      ; Name of the user to filter on.
process=       ; Name of the process to filter on.

[metres]
bars=false

[stats]
cols=pid, process, user, pri, cpu, mem, virt, thrd, disk     
nprocs=-1

[sort]
col=pid

[iterations]
limit=-1

[theme-colour]
background=black
background-highlight=black
error=red
foreground=darkgray
foreground-highlight=white
menubar=gray
range-high=red
range-low=green
range-mid=yellow
process-header=cyan

[theme-mono]
background=black
background-highlight=black
error=gray
foreground=darkgray
foreground-highlight=white
menubar=gray
range-high=gray
range-low=white
range-mid=darkgray
process-header=darkgray
";
    
    [Fact]
    public void Should_Parse_Min_Config_File()
    {
        var configParser = new ConfigParser(MinConfigFile);
        configParser.Parse();
        
        Assert.True(configParser.Sections.Count == 1);
        Assert.Equal("section1", configParser.Sections[0].Name);
        Assert.True(configParser.Sections[0].Contains("key1"));
        Assert.Equal("value1", configParser.Sections[0].GetString("key1"));
        Assert.Equal("value2", configParser.Sections[0].GetString("key2"));
    }

    [Fact]
    public void Should_Parse_Min_Config_File_With_All_DataTypes()
    {
        var configParser = new ConfigParser(MinConfigFileWithAllDataTypes);
        configParser.Parse();
        
        Assert.True(configParser.Sections.Count == 1);
        Assert.Equal("data-types", configParser.Sections[0].Name);
        Assert.Equal("string value", configParser.Sections[0].GetString("string-key"));
        Assert.True(configParser.Sections[0].GetBool("bool-true"));
        Assert.False(configParser.Sections[0].GetBool("bool-false"));
        Assert.Equal(12345678, configParser.Sections[0].GetInt("int-key1"));
        Assert.Equal(-12345678, configParser.Sections[0].GetInt("int-key2"));
        Assert.Equal(ConsoleColor.Black, configParser.Sections[0].GetColour("console-color-black"));
        Assert.Equal(ConsoleColor.DarkBlue, configParser.Sections[0].GetColour("console-color-darkblue"));
        Assert.Equal(ConsoleColor.DarkGreen, configParser.Sections[0].GetColour("console-color-darkgreen"));
        Assert.Equal(ConsoleColor.DarkCyan, configParser.Sections[0].GetColour("console-color-darkcyan"));
        Assert.Equal(ConsoleColor.DarkRed, configParser.Sections[0].GetColour("console-color-darkred"));
        Assert.Equal(ConsoleColor.DarkMagenta, configParser.Sections[0].GetColour("console-color-darkmagenta"));
        Assert.Equal(ConsoleColor.DarkYellow, configParser.Sections[0].GetColour("console-color-darkyellow"));
        Assert.Equal(ConsoleColor.Gray, configParser.Sections[0].GetColour("console-color-gray"));
        Assert.Equal(ConsoleColor.DarkGray, configParser.Sections[0].GetColour("console-color-darkgray"));
        Assert.Equal(ConsoleColor.Blue, configParser.Sections[0].GetColour("console-color-blue"));
        Assert.Equal(ConsoleColor.Green, configParser.Sections[0].GetColour("console-color-green"));
        Assert.Equal(ConsoleColor.Cyan, configParser.Sections[0].GetColour("console-color-cyan"));
        Assert.Equal(ConsoleColor.Red, configParser.Sections[0].GetColour("console-color-red"));
        Assert.Equal(ConsoleColor.Magenta, configParser.Sections[0].GetColour("console-color-magenta"));
        Assert.Equal(ConsoleColor.Yellow, configParser.Sections[0].GetColour("console-color-yellow"));
        Assert.Equal(ConsoleColor.White, configParser.Sections[0].GetColour("console-color-white"));
    }
}
