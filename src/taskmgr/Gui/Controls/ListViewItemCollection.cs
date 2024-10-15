namespace Task.Manager.Gui.Controls;

public sealed class ListViewItemCollection
{
    private readonly ListView _owner;
    
    public ListViewItemCollection(ListView owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }
    
    public void Add(ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        
        _owner.InsertItems([item]);
    }

    public void AddRange(params ListViewItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));
        
        _owner.InsertItems(items);
    }

    public void Clear()
    {
        
    }

    public bool Contains(ListViewItem item)
    {
        return false;
    }

    public void CopyTo(Array array, int index)
    {
        
    }
    
    public int Count { get; }

    // public IEnumerator GetEnumerator()
    // {
    // }

    public int IndexOf(ListViewItem item)
    {
        return -1;
    }

    public void Remove(ListViewItem item)
    {
        
    }
    
    public ListViewItem this[int index]
    {
        // get {
        // }
        set {
            
        }
    }
}