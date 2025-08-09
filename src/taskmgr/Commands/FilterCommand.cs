using Task.Manager.Gui;
using Task.Manager.Gui.Controls;
using Task.Manager.System.Controls.InputBox;

namespace Task.Manager.Commands;

public sealed class FilterCommand(MainScreen mainScreen) : AbstractCommand()
{
    private MainScreen MainScreen { get; } = mainScreen;

    public override void Execute()
    {
        if (!IsEnabled) {
            return;
        }

        void FilterAction(string filter, InputBoxResult result)
        {
            if (result == InputBoxResult.Enter) {
                ProcessControl!.FilterText = filter;
            }
            else if (result == InputBoxResult.Cancel) {
                ProcessControl!.FilterText = string.Empty;
            }

            MainScreen.ShowCommandControl();
            MainScreen.Draw();
        }

        MainScreen.ShowFilterControl(FilterAction);
    }
    
    public override bool IsEnabled
        => ProcessControl != null;

    private ProcessControl? ProcessControl
        => MainScreen.Controls.OfType<ProcessControl>().SingleOrDefault();
}