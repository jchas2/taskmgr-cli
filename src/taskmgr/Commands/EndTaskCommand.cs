using System.Diagnostics;
using Task.Manager.Gui;
using Task.Manager.Gui.Controls;
using Task.Manager.System.Controls.MessageBox;
using Task.Manager.System.Process;

namespace Task.Manager.Commands;

public sealed class EndTaskCommand(MainScreen mainScreen) : ProcessCommand(mainScreen)
{
    private const int EndTaskTimeout = 3000;

    public override void Execute()
    {
        if (!IsEnabled) {
            return;
        }

        int selectedProcessId = SelectedProcessId;
        
        MainScreen.ShowMessageBox(
            "End Task",
            $"Force Task termination with Pid {selectedProcessId}\n\nAre you sure you want to continue?",
            MessageBoxButtons.OkCancel,
            () => ProcessUtils.EndTask(selectedProcessId, EndTaskTimeout));
    }
}
