using Task.Manager.Gui;
using Task.Manager.System.Screens;

namespace Task.Manager.Commands;

public sealed class HelpCommand(ScreenApplication screenApp) : AbstractCommand
{
    public override void Execute() =>
        screenApp.ShowScreen<HelpScreen>();
    
    public override bool IsEnabled => true;
}