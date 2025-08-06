using System.Diagnostics;
using Task.Manager.Gui;
using Task.Manager.Gui.Controls;
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
        bool result = ProcessUtils.EndTask(selectedProcessId, EndTaskTimeout);
        
        if (!result) {
            Trace.WriteLine($"End task {selectedProcessId} did not terminate within timeout of {EndTaskTimeout}");
        }
    }
}
