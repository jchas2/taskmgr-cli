using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public sealed class ThreadsCommand(MainScreen mainScreen) : AbstractCommand
{
    private MainScreen MainScreen { get; } = mainScreen;

    public override void Execute()
    {
        if (false == IsEnabled) {
            return;
        }

        var processControl = MainScreen.GetActiveControl as ProcessControl;
        int pid = processControl!.SelectedProcessId;
        
        MainScreen.SetActiveControl<ThreadsControl>();
        var threadsControl = MainScreen.GetActiveControl as ThreadsControl;
        threadsControl!.SelectedProcessId = pid;
        MainScreen.Draw();
    }

    public override bool IsEnabled =>
        MainScreen.GetActiveControl is ProcessControl control &&
        control.SelectedProcessId > -1;
}

