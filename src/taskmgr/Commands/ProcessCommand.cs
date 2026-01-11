using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public class ProcessCommand(string text, MainScreen mainScreen) : AbstractCommand(text)
{
    protected MainScreen MainScreen { get; } = mainScreen;
    
    public override void Execute() => throw new NotImplementedException();

    public override bool IsEnabled =>
        MainScreen.GetActiveControl is ProcessControl && 
        ProcessControl.SelectedProcessId > -1;

    protected ProcessControl ProcessControl
        => MainScreen.GetControl<ProcessControl>();
    
    internal protected int SelectedProcessId => ProcessControl.SelectedProcessId; 
}
