namespace Task.Manager.Gui.Controls;

public class ListViewColumnHeader
{
    private string _text;
    private const int DefaultColumnWidth = 30;
    
    public ListViewColumnHeader(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        _text = text;
    }

    public ListViewColumnHeader(
        string text,
        ConsoleColor backgroundColor,
        ConsoleColor foregroundColor)
        : this(text)
    {
        BackgroundColour = backgroundColor;
        ForegroundColour = foregroundColor;
    }

    public ConsoleColor BackgroundColour { get; set; }
    
    public ConsoleColor ForegroundColour { get; set; }

    public bool RightAligned { get; set; } = false;
    
    public string Text
    {
        get => _text;
        set => _text = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public int Width { get; set; } = DefaultColumnWidth;
}
