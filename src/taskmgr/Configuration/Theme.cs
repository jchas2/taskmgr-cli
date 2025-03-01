using Task.Manager.System.Configuration;

namespace Task.Manager.Configuration;

public sealed class Theme
{
    private readonly Config _config;

    public Theme(Config config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        var uxSection = _config.GetSection(Constants.Sections.UX);
        var themeSection = _config.GetSection(uxSection.GetString(Constants.Keys.DefaultTheme));
        
        Background = themeSection.GetColour(Constants.Keys.Background, ConsoleColor.Black);
        BackgroundHighlight = themeSection.GetColour(Constants.Keys.BackgroundHighlight, ConsoleColor.Cyan);
        Error = themeSection.GetColour(Constants.Keys.Error, ConsoleColor.Red);
        Foreground = themeSection.GetColour(Constants.Keys.Foreground, ConsoleColor.DarkGray);
        ForegroundHighlight = themeSection.GetColour(Constants.Keys.ForegroundHighlight, ConsoleColor.Black);
        HeaderBackground = themeSection.GetColour(Constants.Keys.HeaderBackground, ConsoleColor.DarkGreen);
        HeaderForeground = themeSection.GetColour(Constants.Keys.HeaderForeground, ConsoleColor.Black);
        Menubar = themeSection.GetColour(Constants.Keys.Menubar, ConsoleColor.Gray);
        RangeHighBackground = themeSection.GetColour(Constants.Keys.RangeHighBackground, ConsoleColor.Red);
        RangeLowBackground = themeSection.GetColour(Constants.Keys.RangeLowBackground, ConsoleColor.Green);
        RangeMidBackground = themeSection.GetColour(Constants.Keys.RangeMidBackground, ConsoleColor.Yellow);
        RangeHighForeground = themeSection.GetColour(Constants.Keys.RangeHighBackground, ConsoleColor.White);
        RangeLowForeground = themeSection.GetColour(Constants.Keys.RangeLowBackground, ConsoleColor.Black);
        RangeMidForeground = themeSection.GetColour(Constants.Keys.RangeMidBackground, ConsoleColor.Black);
    }

    public ConsoleColor Background { get; private set; }
    public ConsoleColor BackgroundHighlight { get; private set; }
    public ConsoleColor Error  { get; private set; }
    public ConsoleColor Foreground { get; private set; }
    public ConsoleColor ForegroundHighlight { get; private set; }
    public ConsoleColor HeaderBackground { get; private set; }
    public ConsoleColor HeaderForeground { get; private set; }
    public ConsoleColor Menubar { get; private set; }
    public ConsoleColor RangeHighBackground { get; private set; }
    public ConsoleColor RangeLowBackground { get; private set; }
    public ConsoleColor RangeMidBackground { get; private set; }
    public ConsoleColor RangeHighForeground { get; private set; }
    public ConsoleColor RangeLowForeground { get; private set; }
    public ConsoleColor RangeMidForeground { get; private set; }
}
