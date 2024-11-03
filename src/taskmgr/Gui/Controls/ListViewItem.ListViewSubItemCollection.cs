using System.Collections;

namespace Task.Manager.Gui.Controls;

public class ListViewSubItemCollection : IEnumerable<ListViewSubItem>
{
    private readonly ListViewItem _owner;
    
    public ListViewSubItemCollection(ListViewItem owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }
    
    public void Add(ListViewSubItem subItem)
    {
        ArgumentNullException.ThrowIfNull(subItem, nameof(subItem));
        _owner.InsertSubItems([subItem]);
    }

    public void AddRange(params ListViewSubItem[] subItems)
    {
        ArgumentNullException.ThrowIfNull(subItems, nameof(subItems));
        _owner.InsertSubItems(subItems);
    }

    public void Clear() => _owner.ClearSubItems();

    public bool Contains(ListViewSubItem subItem)
    {
        ArgumentNullException.ThrowIfNull(subItem, nameof(subItem));
        return _owner.Contains(subItem);
    }

    public IEnumerator<ListViewSubItem> GetEnumerator()
    {
        /* Shallow copy the subitems and return an enumerator off that container. */
        var subItems = new List<ListViewSubItem>(_owner.SubItemCount);
        
        for (int i = 0; i < _owner.SubItemCount; i++) {
            subItems[i] = _owner.GetSubItemByIndex(i);
        }
        
        return subItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(ListViewSubItem subItem)
    {
        ArgumentNullException.ThrowIfNull(subItem, nameof(subItem));
        return _owner.IndexOfSubItem(subItem);
    }
    
    public ListViewSubItem this[int index]
    {
        get {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            return this[index];
        }
        set {
            throw new InvalidOperationException();
        }
    }
}
