using System.Diagnostics;
using System.Reflection;
using Task.Manager.System.Process;

namespace Task.Manager.System.Tests.Process;

public sealed class ProcessTimeInfoTests
{
    [Fact]
    public void Should_Clear_All_Properties()
    {
        const int FieldsTested = 3;

        var fields = typeof(ProcessTimeInfo).GetFields(BindingFlags.Public | BindingFlags.Instance);
        Assert.Equal(FieldsTested, fields.Length);

        var processTimeInfo = new ProcessTimeInfo() {
            DiskOperations = 78346578346,
            KernelTime = 38346510925,
            UserTime = 72346378346
        };
        
        processTimeInfo.Clear();
        
        Assert.True(processTimeInfo.DiskOperations == 0);
        Assert.True(processTimeInfo.KernelTime == 0);
        Assert.True(processTimeInfo.UserTime == 0);
    }
}