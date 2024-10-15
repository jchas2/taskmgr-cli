namespace Task.Manager.Gui.Controls;

public class ListView
{
    private readonly ListViewItemCollection _itemCollection;
    private List<ListViewItem> _items = [];

    public ListView()
    {
        _itemCollection = new ListViewItemCollection(this);
    }
    
    public ConsoleColor BackgroundColour { get; set; }

    public ConsoleColor ForegroundColour { get; set; }

    public void InsertItems(ListViewItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
        
        _items.AddRange(items);
    }
    
    public ListViewItemCollection Items => _itemCollection;
}