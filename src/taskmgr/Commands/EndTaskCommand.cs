using Task.Manager.Configuration;
using Task.Manager.Gui;
using Task.Manager.System.Controls.MessageBox;
using Task.Manager.System.Process;

namespace Task.Manager.Commands;

public sealed class EndTaskCommand(
    string text, 
    MainScreen mainScreen,
    AppConfig appConfig) : ProcessCommand(text, mainScreen)
{
    private const int EndTaskTimeout = 3000;

    public override void Execute()
    {
        if (!IsEnabled) {
            return;
        }

        int selectedProcessId = SelectedProcessId;
        Action action = () => ProcessUtils.EndTask(selectedProcessId, EndTaskTimeout);

        if (appConfig.ConfirmTaskDelete) {
            MainScreen.ShowMessageBox(
                "End Task",
                $"Force Task termination with Pid {selectedProcessId}\n\nAre you sure you want to continue?",
                MessageBoxButtons.OkCancel,
                action);
        }
        else {
            action.Invoke();
        }        
    }
}
