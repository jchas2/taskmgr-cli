using System.Collections;

namespace Task.Manager.System.Controls.ListView;

public sealed class ListViewItemCollection : IEnumerable<ListViewItem>
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

    public void Clear() => _owner.ClearItems();

    public bool Contains(ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        return _owner.Contains(item);
    }

    public int Count => _owner.ItemCount;

    public IEnumerator<ListViewItem> GetEnumerator()
    {
        /* Shallow copy the items and return an enumerator off that container. */
        var items = new List<ListViewItem>(_owner.Items.Count);
        
        for (int i = 0; i < _owner.ItemCount; i++) {
            items[i] = _owner.GetItemByIndex(i);
        }
        
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        return _owner.IndexOfItem(item);
    }

    public void InsertAt(int index, ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _owner.Items.Count, nameof(index));
        _owner.InsertItem(index, item);
    }
    
    public void Remove(ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        _owner.RemoveItem(item);
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
