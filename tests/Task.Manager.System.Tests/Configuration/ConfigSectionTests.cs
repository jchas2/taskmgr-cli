using Task.Manager.System.Configuration;

namespace Task.Manager.System.Tests.Configuration;

public class ConfigSectionTests
{
    [Fact]
    public void Should_Add_Section_With_Strings()
    {
        var section = new ConfigSection("Strings")
            .Add("Key1", "Value1")
            .Add("Key2", "Value2")
            .Add("Key3", "Value3");
        
        Assert.Equal("Value1", section.GetString("Key1"));
        Assert.Equal("Value2", section.GetString("Key2"));
        Assert.Equal("Value3", section.GetString("Key3"));
    }

    [Fact]
    public void Should_Add_Section_With_Value_Types()
    {
        var section = new ConfigSection("Value-Types")
            .Add("Key1", "12345678")
            .Add("Key2", "-12345678")
            .Add("Key3", "true")
            .Add("Key4", "false");
        
        Assert.Equal(12345678, section.GetInt("Key1"));
        Assert.Equal(-12345678, section.GetInt("Key2"));
        Assert.True(section.GetBool("Key3"));
        Assert.False(section.GetBool("Key4"));
    }
}