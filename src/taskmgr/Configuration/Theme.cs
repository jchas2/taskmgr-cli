using Task.Manager.System.Configuration;

namespace Task.Manager.Configuration;

public sealed class Theme
{
    private ConfigSection? themeSection;

    public Theme() { }

    public Theme(ConfigSection configSection) => themeSection = configSection;

    public string Name => themeSection?.Name ?? string.Empty;

    public ConsoleColor Background => themeSection?.GetColour(Constants.Keys.Background, ConsoleColor.Black) ?? ConsoleColor.Black;
    public ConsoleColor BackgroundHighlight => themeSection?.GetColour(Constants.Keys.BackgroundHighlight, ConsoleColor.Cyan) ?? ConsoleColor.Cyan;
    public ConsoleColor ColumnCommandNormalUserSpace => themeSection?.GetColour(Constants.Keys.ColCmdNormalUserSpace, ConsoleColor.Green) ?? ConsoleColor.Green;
    public ConsoleColor ColumnCommandLowPriority => themeSection?.GetColour(Constants.Keys.ColCmdLowPriority, ConsoleColor.Blue) ?? ConsoleColor.Blue;
    public ConsoleColor ColumnCommandHighCpu => themeSection?.GetColour(Constants.Keys.ColCmdHighCpu, ConsoleColor.Red) ?? ConsoleColor.Red;
    public ConsoleColor ColumnCommandIoBound => themeSection?.GetColour(Constants.Keys.ColCmdIoBound, ConsoleColor.Cyan) ?? ConsoleColor.Cyan;
    public ConsoleColor ColumnCommandScript => themeSection?.GetColour(Constants.Keys.ColCmdScript, ConsoleColor.Yellow) ?? ConsoleColor.Yellow;
    public ConsoleColor ColumnUserCurrentNonRoot => themeSection?.GetColour(Constants.Keys.ColUserCurrentNonRoot, ConsoleColor.Green) ?? ConsoleColor.Green;
    public ConsoleColor ColumnUserOtherNonRoot => themeSection?.GetColour(Constants.Keys.ColUserOtherNonRoot, ConsoleColor.Magenta) ?? ConsoleColor.Magenta;
    public ConsoleColor ColumnUserSystem => themeSection?.GetColour(Constants.Keys.ColUserSystem, ConsoleColor.Gray) ?? ConsoleColor.Gray;
    public ConsoleColor ColumnUserRoot => themeSection?.GetColour(Constants.Keys.ColUserRoot, ConsoleColor.White) ?? ConsoleColor.White;
    public ConsoleColor CommandBackground => themeSection?.GetColour(Constants.Keys.CommandBackground, ConsoleColor.Cyan) ?? ConsoleColor.Cyan;
    public ConsoleColor CommandForeground => themeSection?.GetColour(Constants.Keys.CommandForeground, ConsoleColor.White) ?? ConsoleColor.White;
    public ConsoleColor Error => themeSection?.GetColour(Constants.Keys.Error, ConsoleColor.Red) ?? ConsoleColor.Red;
    public ConsoleColor Foreground => themeSection?.GetColour(Constants.Keys.Foreground, ConsoleColor.White) ?? ConsoleColor.White;
    public ConsoleColor ForegroundHighlight => themeSection?.GetColour(Constants.Keys.ForegroundHighlight, ConsoleColor.Black) ?? ConsoleColor.Black;
    public ConsoleColor HeaderBackground => themeSection?.GetColour(Constants.Keys.HeaderBackground, ConsoleColor.DarkGreen) ?? ConsoleColor.DarkGreen;
    public ConsoleColor HeaderForeground => themeSection?.GetColour(Constants.Keys.HeaderForeground, ConsoleColor.Black) ?? ConsoleColor.Black;
    public ConsoleColor MenubarBackground => themeSection?.GetColour(Constants.Keys.MenubarBackground, ConsoleColor.DarkBlue) ?? ConsoleColor.DarkBlue;
    public ConsoleColor MenubarForeground => themeSection?.GetColour(Constants.Keys.MenubarForeground, ConsoleColor.Gray) ?? ConsoleColor.Gray;
    public ConsoleColor RangeHighBackground => themeSection?.GetColour(Constants.Keys.RangeHighBackground, ConsoleColor.Red) ?? ConsoleColor.Red;
    public ConsoleColor RangeLowBackground => themeSection?.GetColour(Constants.Keys.RangeLowBackground, ConsoleColor.Green) ?? ConsoleColor.Green;
    public ConsoleColor RangeMidBackground => themeSection?.GetColour(Constants.Keys.RangeMidBackground, ConsoleColor.Yellow) ?? ConsoleColor.Yellow;
    public ConsoleColor RangeHighForeground => themeSection?.GetColour(Constants.Keys.RangeHighBackground, ConsoleColor.White) ?? ConsoleColor.White;
    public ConsoleColor RangeLowForeground => themeSection?.GetColour(Constants.Keys.RangeLowBackground, ConsoleColor.Black) ?? ConsoleColor.Black;
    public ConsoleColor RangeMidForeground => themeSection?.GetColour(Constants.Keys.RangeMidBackground, ConsoleColor.Black) ?? ConsoleColor.Black;
}
