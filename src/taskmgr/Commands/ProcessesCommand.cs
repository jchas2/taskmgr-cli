using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public sealed class ProcessesCommand(MainScreen mainScreen) : AbstractCommand
{
    private MainScreen MainScreen { get; } = mainScreen;

    public override void Execute()
    {
        MainScreen.SetActiveControl<ProcessControl>();
        MainScreen.Draw();
    }

    public override bool IsEnabled => true;
}