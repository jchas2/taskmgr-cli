using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public class ProcessSortCommand(string text, MainScreen mainScreen) : AbstractCommand(text) 
{
    public MainScreen MainScreen { get; } = mainScreen;

    public override void Execute()
    {
        if (!IsEnabled) {
            return;
        }

        var processControl = MainScreen.GetActiveControl as ProcessControl;
        processControl!.SetMode(ProcessControl.ControlMode.SortSelection);
    }

    public override bool IsEnabled  => 
        MainScreen.GetActiveControl is ProcessControl;
}