using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public class ProcessCommand(MainScreen mainScreen) : AbstractCommand
{
    protected MainScreen MainScreen { get; } = mainScreen;
    
    public override void Execute() => throw new NotImplementedException();

    public override bool IsEnabled =>
        MainScreen.GetActiveControl is ProcessControl && 
        ProcessControl.SelectedProcessId > -1;

    protected ProcessControl ProcessControl
        => MainScreen.GetControl<ProcessControl>();
    
    protected int SelectedProcessId => ProcessControl.SelectedProcessId; 
}
