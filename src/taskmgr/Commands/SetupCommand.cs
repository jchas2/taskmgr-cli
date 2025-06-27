using Task.Manager.Gui;
using Task.Manager.System.Screens;

namespace Task.Manager.Commands;

public sealed class SetupCommand : AbstractCommand
{
    public override void Execute() =>
        ScreenApplication.ShowScreen<SetupScreen>();
    
    public override bool IsEnabled => true;
}
