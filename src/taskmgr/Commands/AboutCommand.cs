using Task.Manager.Cli.Utils;
using Task.Manager.Gui;
using Task.Manager.System.Controls.MessageBox;

namespace Task.Manager.Commands;

public sealed class AboutCommand(string text, MainScreen mainScreen) : AbstractCommand(text)
{
    public override void Execute()
    {
        string version = AssemblyVersionInfo.GetVersion();

        mainScreen.ShowMessageBox(
            "About Task Manager",
            $"Designed and Programmed by Jason Chase\n\nVersion {version}",
            MessageBoxButtons.Ok,
            () => { });
    }

    public override bool IsEnabled => true;
}
