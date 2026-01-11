using Task.Manager.Gui;
using Task.Manager.System.Screens;

namespace Task.Manager.Tests.Commands;

public static class CommandHelper
{
    public static MainScreen SetupMainScreen() => SetupMainScreenWithAll().mainScreen;
    
    public static (RunContext context, MainScreen mainScreen) SetupMainScreenWithContext()
    {
        var (context, _, mainScreen) = SetupMainScreenWithAll();
        return (context, mainScreen);
    }

    public static (ScreenApplication screenApp, MainScreen mainScreen) SetupMainScreenWithScreenApp()
    {
        var (_, screenApp, mainScreen) = SetupMainScreenWithAll();
        return (screenApp, mainScreen);
    }

    private static (RunContext context, ScreenApplication screenApp, MainScreen mainScreen) SetupMainScreenWithAll()
    {
        var context = new RunContextHelper().GetRunContext();
        var screenApp = new ScreenApplication(context.Terminal);
        var mainScreen = new MainScreen(screenApp, context);

        return (context, screenApp, mainScreen);
    }
}
