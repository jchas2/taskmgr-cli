namespace Task.Manager.Gui.Controls;

public class ListViewItem
{
    private List<ListViewSubItem> _subItems = [];

    public ListViewItem(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        
        _subItems.Add(new ListViewSubItem(this, text));
    }

    public ListViewItem(string[] items)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        for (int i = 0; i < items.Length; i++) {
            ArgumentNullException.ThrowIfNull(items[i], nameof(items));
            _subItems.Add(new ListViewSubItem(this, items[i]));
        }
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
    
    internal ListView? ListView { get; }

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
