using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public sealed class ThreadsCommand(MainScreen mainScreen) : ProcessCommand(mainScreen)
{
    public override void Execute()
    {
        if (!IsEnabled) {
            return;
        }

        var threadsControl = MainScreen.SetActiveControl<ThreadsControl>();
        threadsControl.SelectedProcessId = SelectedProcessId;
        MainScreen.Draw();
    }
}
