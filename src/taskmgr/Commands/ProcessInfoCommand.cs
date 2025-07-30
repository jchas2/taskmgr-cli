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

        var processInfoControl = MainScreen.SetActiveControl<ProcessInfoControl>();
        processInfoControl.SelectedProcessId = SelectedProcessId;
        MainScreen.Draw();
    }
}
