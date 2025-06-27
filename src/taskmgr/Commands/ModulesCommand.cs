using Task.Manager.Gui;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Commands;

public sealed class ModulesCommand(MainScreen mainScreen) : ProcessCommand(mainScreen)
{
    public override void Execute()
    {
        if (false == IsEnabled) {
            return;
        }

        var modulesControl = MainScreen.SetActiveControl<ModulesControl>() as ModulesControl;
        modulesControl!.SelectedProcessId = SelectedProcessId;
        MainScreen.Draw();
    }
}