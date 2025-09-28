using Task.Manager.Gui;
using Task.Manager.System.Screens;

namespace Task.Manager.Commands;

public sealed class SetupCommand(string text, ScreenApplication screenApp) : AbstractCommand(text)
{
    public override void Execute() =>
        screenApp.ShowScreen<SetupScreen>();
    
    public override bool IsEnabled => true;
}
