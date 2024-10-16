namespace Task.Manager.Gui.Controls;

public class ListView
{
    /* 
     * The _itemCollection acts as a proxy for updates to the underlying _items List.
     * This provides a clean api for interacting with an Items collection on the ListView
     * control, similar to the old Win32 ListView common control. 
     */
    private readonly ListViewItemCollection _itemCollection;

    /* The container holding the items for rendering. We don't expose this via a public api. */
    private List<ListViewItem> _items = [];

    public ListView()
    {
        _itemCollection = new ListViewItemCollection(this);
    }
    
    public ConsoleColor BackgroundColour { get; set; }

    internal void Clear()
    {
        _items.Clear();
    }

    internal bool Contains(ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        return _items.Contains(item);
    }
    
    public ConsoleColor ForegroundColour { get; set; }

    internal ListViewItem GetItemByIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Count, nameof(index));

        return _items[index];
    }

    internal void InsertItem(int index, ListViewItem item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _items.Count, nameof(index));
        
        _items.Insert(index, item);
    }
    
    internal void InsertItems(ListViewItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _items.AddRange(items);
    }
    
    public ListViewItemCollection Items => _itemCollection;

    internal void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Count, nameof(index));
            
        _items.RemoveAt(index);
    }
}