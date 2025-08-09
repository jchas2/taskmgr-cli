using Task.Manager.Gui;
using Task.Manager.System.Controls.InputBox;

namespace Task.Manager.Commands;

public class FilterCommand(MainScreen mainScreen) : ProcessCommand(mainScreen)
{
    public override void Execute()
    {
        if (!IsEnabled) {
            return;
        }

        Action<string, InputBoxResult> filterAction = (filter, result) => {
            if (result == InputBoxResult.Enter) {
                ProcessControl.FilterText = filter;
            }
            else if (result == InputBoxResult.Cancel) {
                ProcessControl.FilterText = string.Empty;
            }
            
            MainScreen.ShowCommandControl();
            MainScreen.Draw();
        };
        
        MainScreen.ShowFilterControl(filterAction);
    }
}