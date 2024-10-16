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

    public void Clear() => _owner.Items.Clear();

    public bool Contains(ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        return _owner.Contains(item);
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
        get {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            return _owner.GetItemByIndex(index);
        }
        set {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            _owner.RemoveAt(index);
            _owner.InsertItem(index, value);
        }
    }
}
