using Task.Manager.System.Process;

namespace Task.Manager.System.Tests.Process;

public sealed class ModuleInfoTests
{
    [Fact]
    public void ModuleInfo_Write_Read_Test()
    {
        ModuleInfo moduleInfo = new() {
            ModuleName = "test",
            FileName = "test.dll"
        };
        
        Assert.Equal("test", moduleInfo.ModuleName);
        Assert.Equal("test.dll", moduleInfo.FileName);
    }
}