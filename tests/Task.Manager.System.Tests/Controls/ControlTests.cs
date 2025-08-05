using Moq;
using Task.Manager.System.Controls;

namespace Task.Manager.System.Tests.Controls;

public class ControlTests
{
    private static readonly Mock<ISystemTerminal> SystemTerminalSingleton = new();
    
    public static List<Control> GetControlData()
        => new() {
            new Control(SystemTerminalSingleton.Object),
            new Control(SystemTerminalSingleton.Object),
            new Control(SystemTerminalSingleton.Object)
        };

    [Fact]
    public void Should_Add_All_Items()
    {
        var control = new Control(SystemTerminalSingleton.Object);
        control.Controls.AddRange(GetControlData().ToArray());
        
        Assert.True(control.Controls.Count == 3);
    }
}
