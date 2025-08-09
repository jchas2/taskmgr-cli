using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public class ProcessCommand(MainScreen mainScreen) : AbstractCommand
{
    protected MainScreen MainScreen { get; } = mainScreen;

    public override void Execute() => throw new NotImplementedException();

    public override bool IsEnabled
        => ProcessControl.SelectedProcessId > -1;

    protected ProcessControl ProcessControl
        => MainScreen.Controls.OfType<ProcessControl>().Single();
    
    protected int SelectedProcessId => ProcessControl.SelectedProcessId; 
}