using Task.Manager.System.Configuration;

namespace Task.Manager.Configuration;

public sealed class Theme
{
    public Theme(Config config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        ConfigSection uxSection = config.GetConfigSection(Constants.Sections.UX);
        ConfigSection themeSection = config.GetConfigSection(uxSection.GetString(Constants.Keys.DefaultTheme));
        
        Background = themeSection.GetColour(Constants.Keys.Background, ConsoleColor.Black);
        BackgroundHighlight = themeSection.GetColour(Constants.Keys.BackgroundHighlight, ConsoleColor.Cyan);
        ColumnCommandNormalUserSpace = themeSection.GetColour(Constants.Keys.ColCmdNormalUserSpace, ConsoleColor.Green);
        ColumnCommandLowPriority = themeSection.GetColour(Constants.Keys.ColCmdLowPriority, ConsoleColor.Blue);
        ColumnCommandHighCpu = themeSection.GetColour(Constants.Keys.ColCmdHighCpu, ConsoleColor.Red);
        ColumnCommandIoBound = themeSection.GetColour(Constants.Keys.ColCmdIoBound, ConsoleColor.Cyan);
        ColumnCommandScript = themeSection.GetColour(Constants.Keys.ColCmdScript, ConsoleColor.Yellow);
        ColumnUserCurrentNonRoot = themeSection.GetColour(Constants.Keys.ColUserCurrentNonRoot, ConsoleColor.Green);
        ColumnUserOtherNonRoot = themeSection.GetColour(Constants.Keys.ColUserOtherNonRoot, ConsoleColor.Magenta);
        ColumnUserSystem = themeSection.GetColour(Constants.Keys.ColUserSystem, ConsoleColor.Gray);
        ColumnUserRoot = themeSection.GetColour(Constants.Keys.ColUserRoot, ConsoleColor.White);
        CommandBackground = themeSection.GetColour(Constants.Keys.CommandBackground, ConsoleColor.Cyan);
        CommandForeground = themeSection.GetColour(Constants.Keys.CommandForeground, ConsoleColor.White);
        Error = themeSection.GetColour(Constants.Keys.Error, ConsoleColor.Red);
        Foreground = themeSection.GetColour(Constants.Keys.Foreground, ConsoleColor.White);
        ForegroundHighlight = themeSection.GetColour(Constants.Keys.ForegroundHighlight, ConsoleColor.Black);
        HeaderBackground = themeSection.GetColour(Constants.Keys.HeaderBackground, ConsoleColor.DarkGreen);
        HeaderForeground = themeSection.GetColour(Constants.Keys.HeaderForeground, ConsoleColor.Black);
        MenubarBackground = themeSection.GetColour(Constants.Keys.MenubarBackground, ConsoleColor.DarkBlue);
        MenubarForeground = themeSection.GetColour(Constants.Keys.MenubarForeground, ConsoleColor.Gray);
        RangeHighBackground = themeSection.GetColour(Constants.Keys.RangeHighBackground, ConsoleColor.Red);
        RangeLowBackground = themeSection.GetColour(Constants.Keys.RangeLowBackground, ConsoleColor.Green);
        RangeMidBackground = themeSection.GetColour(Constants.Keys.RangeMidBackground, ConsoleColor.Yellow);
        RangeHighForeground = themeSection.GetColour(Constants.Keys.RangeHighBackground, ConsoleColor.White);
        RangeLowForeground = themeSection.GetColour(Constants.Keys.RangeLowBackground, ConsoleColor.Black);
        RangeMidForeground = themeSection.GetColour(Constants.Keys.RangeMidBackground, ConsoleColor.Black);
    }

    public ConsoleColor Background { get; private set; }
    public ConsoleColor BackgroundHighlight { get; private set; }
    
    public ConsoleColor ColumnCommandNormalUserSpace { get; private set; }
    public ConsoleColor ColumnCommandLowPriority { get; private set; }
    public ConsoleColor ColumnCommandHighCpu { get; private set; }
    public ConsoleColor ColumnCommandIoBound { get; private set; }
    public ConsoleColor ColumnCommandScript { get; private set; }
    public ConsoleColor ColumnUserCurrentNonRoot { get; private set; }
    public ConsoleColor ColumnUserOtherNonRoot { get; private set; }
    public ConsoleColor ColumnUserSystem { get; private set; }
    public ConsoleColor ColumnUserRoot { get; private set; }
    
    public ConsoleColor CommandBackground { get; private set; }
    public ConsoleColor CommandForeground { get; private set; }
    public ConsoleColor Error  { get; private set; }
    public ConsoleColor Foreground { get; private set; }
    public ConsoleColor ForegroundHighlight { get; private set; }
    public ConsoleColor HeaderBackground { get; private set; }
    public ConsoleColor HeaderForeground { get; private set; }
    public ConsoleColor MenubarForeground { get; private set; }
    public ConsoleColor MenubarBackground { get; private set; }
    public ConsoleColor RangeHighBackground { get; private set; }
    public ConsoleColor RangeLowBackground { get; private set; }
    public ConsoleColor RangeMidBackground { get; private set; }
    public ConsoleColor RangeHighForeground { get; private set; }
    public ConsoleColor RangeLowForeground { get; private set; }
    public ConsoleColor RangeMidForeground { get; private set; }
}
