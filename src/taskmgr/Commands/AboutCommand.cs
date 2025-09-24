using Task.Manager.Gui;
using Task.Manager.System.Controls.MessageBox;
using Task.Manager.System.Process;

namespace Task.Manager.Commands;

public sealed class AboutCommand(MainScreen mainScreen) : AbstractCommand
{
    public override void Execute()
    {
        mainScreen.ShowMessageBox(
            "About Task Manager",
            $"Designed and Programmed by Jason Chase\n\nVersion 1.0.0.0",
            MessageBoxButtons.Ok,
            () => { });
    }

    public override bool IsEnabled => true;
}
