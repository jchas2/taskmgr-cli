namespace Task.Manager.Gui.Controls;

public class ListViewItem
{
    private List<ListViewSubItem> _subItems = [];

    public ListViewItem(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        
        _subItems.Add(new ListViewSubItem(this, text));
    }

    public ListViewItem(
        string text,
        ConsoleColor backgroundColor,
        ConsoleColor foregroundColor)
        : this(text)
    {
        BackgroundColour = backgroundColor;
        ForegroundColour = foregroundColor;
    }
    
    public ListViewItem(string[] items)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        for (int i = 0; i < items.Length; i++) {
            ArgumentNullException.ThrowIfNull(items[i], nameof(items));
            _subItems.Add(new ListViewSubItem(this, items[i]));
        }
    }

    public ListViewItem(
        string[] items,
        ConsoleColor backgroundColor,
        ConsoleColor foregroundColor)
        : this(items)
    {
        BackgroundColour = backgroundColor;
        ForegroundColour = foregroundColor;
    }

    public ListViewItem(ListViewSubItem[] subItems)
    {
        ArgumentNullException.ThrowIfNull(subItems, nameof(subItems));

        for (int i = 0; i < subItems.Length; i++) {
            ArgumentNullException.ThrowIfNull(subItems[i], nameof(subItems));
            subItems[i].Owner = this;
            _subItems.Add(subItems[i]);
        }
    }

    public ListViewItem(
        ListViewSubItem[] subItems,
        ConsoleColor backgroundColor,
        ConsoleColor foregroundColor)
        : this(subItems)
    {
        BackgroundColour = backgroundColor;
        ForegroundColour = foregroundColor;
    }
    
    public ConsoleColor ForegroundColour
    {
        get => _subItems[0].ForegroundColor;
        set => _subItems[0].ForegroundColor = value;
    }

    public ConsoleColor BackgroundColour
    {
        get => _subItems[0].BackgroundColor;
        set => _subItems[0].BackgroundColor = value;
    }
}
