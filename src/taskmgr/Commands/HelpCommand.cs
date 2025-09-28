using Task.Manager.Gui;
using Task.Manager.System.Screens;

namespace Task.Manager.Commands;

public sealed class HelpCommand(string text, ScreenApplication screenApp) : AbstractCommand(text)
{
    public override void Execute() =>
        screenApp.ShowScreen<HelpScreen>();
    
    public override bool IsEnabled => true;
}