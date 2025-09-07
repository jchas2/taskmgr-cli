using Task.Manager.Gui;
using Task.Manager.System.Screens;

namespace Task.Manager.Commands;

public sealed class SetupCommand(ScreenApplication screenApp) : AbstractCommand
{
    public override void Execute() =>
        screenApp.ShowScreen<SetupScreen>();
    
    public override bool IsEnabled => true;
}
