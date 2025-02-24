namespace Task.Manager.System.Controls.ListView;

public class ListViewItem
{
    /*
     * The Collection acts as a proxy for updates to the underlying List<T>.
     * This provides a clean api for interacting with Collections on the ListViewItem
     * control, similar to the Win32 ListView common control.
     */
    private readonly ListViewSubItemCollection _subItemCollection;

    /* The containers holding the List<T> for rendering. We don't expose them via a public api. */
    private List<ListViewSubItem> _subItems = [];

    public ListViewItem(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        
        _subItems.Add(new ListViewSubItem(this, text));
        _subItemCollection = new ListViewSubItemCollection(this);
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
        
        _subItemCollection = new ListViewSubItemCollection(this);
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
        
        _subItemCollection = new ListViewSubItemCollection(this);
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

    public ConsoleColor BackgroundColour { get; set; }

    internal void ClearSubItems() => _subItems.Clear();

    internal bool Contains(ListViewSubItem subItem)
    {
        ArgumentNullException.ThrowIfNull(subItem, nameof(subItem));
        return _subItems.Contains(subItem);
    }

    public ConsoleColor ForegroundColour { get; set; }

    internal ListViewSubItem GetSubItemByIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _subItems.Count, nameof(index));
        return _subItems[index];
    }
    
    internal int IndexOfSubItem(ListViewSubItem subItem)
    {
        ArgumentNullException.ThrowIfNull(subItem, nameof(subItem));

        for (int i = 0; i < _subItems.Count; i++) {
            if (_subItems[i] == subItem) {
                return i;
            }
        }

        return -1;
    }

    internal void InsertSubItems(ListViewSubItem[] subItems)
    {
        ArgumentNullException.ThrowIfNull(subItems, nameof(subItems));
        _subItems.AddRange(subItems);
    }
    
    internal int SubItemCount => _subItems.Count;
    
    public ListViewSubItemCollection SubItems => _subItemCollection;

    public string Text
    {
        get => _subItems[0].Text;
        set => _subItems[0].Text = value;
    }
}
