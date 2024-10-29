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
        BackgroundHighlight = themeSection.GetColour(Constants.Keys.BackgroundHighlight, ConsoleColor.Black);
        Error = themeSection.GetColour(Constants.Keys.Error, ConsoleColor.Red);
        Foreground = themeSection.GetColour(Constants.Keys.Foreground, ConsoleColor.DarkGray);
        ForegroundHighlight = themeSection.GetColour(Constants.Keys.ForegroundHighlight, ConsoleColor.White);
        HeaderBackground = themeSection.GetColour(Constants.Keys.HeaderBackground, ConsoleColor.DarkCyan);
        HeaderForeground = themeSection.GetColour(Constants.Keys.HeaderForeground, ConsoleColor.White);
        Menubar = themeSection.GetColour(Constants.Keys.Menubar, ConsoleColor.Gray);
        RangeHigh = themeSection.GetColour(Constants.Keys.RangeHigh, ConsoleColor.Red);
        RangeLow = themeSection.GetColour(Constants.Keys.RangeLow, ConsoleColor.Green);
        RangeMid = themeSection.GetColour(Constants.Keys.RangeMid, ConsoleColor.Yellow);
    }

    public ConsoleColor Background { get; private set; }
    public ConsoleColor BackgroundHighlight { get; private set; }
    public ConsoleColor Error  { get; private set; }
    public ConsoleColor Foreground { get; private set; }
    public ConsoleColor ForegroundHighlight { get; private set; }
    public ConsoleColor HeaderBackground { get; private set; }
    public ConsoleColor HeaderForeground { get; private set; }
    public ConsoleColor Menubar { get; private set; }
    public ConsoleColor RangeHigh { get; private set; }
    public ConsoleColor RangeLow { get; private set; }
    public ConsoleColor RangeMid { get; private set; }
}
