namespace Task.Manager.Cli.Utils;

public sealed class TerminalColourRestorer : IDisposable
{
    private readonly ConsoleColor _cachedBackgroundColour;
    private readonly ConsoleColor _cachedForegroundColour;

    public TerminalColourRestorer()
    {
        _cachedBackgroundColour = Console.BackgroundColor;
        _cachedForegroundColour = Console.ForegroundColor;
    }
    
    public void Dispose()
    {
        Console.BackgroundColor = _cachedBackgroundColour;
        Console.ForegroundColor = _cachedForegroundColour;
    }
}