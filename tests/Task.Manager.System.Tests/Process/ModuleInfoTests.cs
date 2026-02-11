using System.Reflection;
using Task.Manager.System.Process;

namespace Task.Manager.System.Tests.Process;

public sealed class ModuleInfoTests
{
    [Fact]
    public void ModuleInfo_Canary_Test()
    {
        // If this fails, review all tests in this class                                                                                   
        // and update the expected count after adding tests                                                                                
        const int ExpectedPropertyCount = 2;

        int actualCount = typeof(ModuleInfo).GetProperties(
            BindingFlags.Public | BindingFlags.Instance).Length;

        Assert.Equal(ExpectedPropertyCount, actualCount);
    }
    
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