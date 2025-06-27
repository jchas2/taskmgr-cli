using Task.Manager.Gui;
using Task.Manager.System.Screens;

namespace Task.Manager.Commands;

public sealed class HelpCommand : AbstractCommand
{
    public override void Execute() =>
        ScreenApplication.ShowScreen<HelpScreen>();
    
    public override bool IsEnabled => true;
}