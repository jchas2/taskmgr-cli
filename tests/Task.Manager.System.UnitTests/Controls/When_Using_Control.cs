using Task.Manager.System.Controls;

namespace Task.Manager.System.UnitTests.Controls;

public class When_Using_Control
{
    private static readonly SystemTerminal SystemTerminalSingleton = new SystemTerminal();
    
    public static List<Control> GetControlData()
        => new() {
            new Control(SystemTerminalSingleton),
            new Control(SystemTerminalSingleton),
            new Control(SystemTerminalSingleton)
        };

    [Fact]
    public void Should_Add_All_Items()
    {
        var control = new Control(SystemTerminalSingleton);
        control.Controls.AddRange(GetControlData().ToArray());
        
        Assert.True(control.Controls.Count == 3);
    }
}
