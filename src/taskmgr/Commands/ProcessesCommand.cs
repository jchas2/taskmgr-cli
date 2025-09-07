using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public sealed class ProcessesCommand(MainScreen mainScreen) : AbstractCommand
{
    public override void Execute()
    {
        _ = mainScreen.SetActiveControl<ProcessControl>();
        mainScreen.Draw();
    }

    public override bool IsEnabled => true;
}
