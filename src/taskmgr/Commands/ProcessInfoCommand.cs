using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public sealed class ProcessInfoCommand(MainScreen mainScreen) : ProcessCommand(mainScreen)
{
    public override void Execute()
    {
        if (!IsEnabled) {
            return;
        }

        var processInfoControl = MainScreen.GetControl<ProcessInfoControl>();
        processInfoControl.SelectedProcessId = SelectedProcessId;
        MainScreen.SetActiveControl<ProcessInfoControl>();
        MainScreen.Draw();
    }
}
