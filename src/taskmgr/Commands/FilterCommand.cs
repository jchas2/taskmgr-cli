using Task.Manager.Gui;
using Task.Manager.Gui.Controls;
using Task.Manager.System.Controls.InputBox;

namespace Task.Manager.Commands;

public sealed class FilterCommand(MainScreen mainScreen) : AbstractCommand()
{
    public override void Execute()
    {
        if (!IsEnabled) {
            return;
        }

        void FilterAction(string filter, InputBoxResult result)
        {
            if (result == InputBoxResult.Enter) {
                ProcessControl.FilterText = filter;
            }
            else if (result == InputBoxResult.Cancel) {
                ProcessControl.FilterText = string.Empty;
            }

            mainScreen.ShowCommandControl();
            mainScreen.Draw();
        }

        mainScreen.ShowFilterControl(FilterAction);
    }

    public override bool IsEnabled
        => mainScreen.GetActiveControl is ProcessControl;

    private ProcessControl ProcessControl
        => mainScreen.GetControl<ProcessControl>();
}