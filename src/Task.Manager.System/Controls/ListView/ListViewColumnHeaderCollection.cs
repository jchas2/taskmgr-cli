﻿using System.Collections;

namespace Task.Manager.System.Controls.ListView;

public class ListViewColumnHeaderCollection : IEnumerable<ListViewColumnHeader>
{
    private readonly ListView _owner;
    
    public ListViewColumnHeaderCollection(ListView owner) =>
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    
    public ListViewColumnHeaderCollection Add(ListViewColumnHeader columnHeader)
    {
        ArgumentNullException.ThrowIfNull(columnHeader, nameof(columnHeader));
        
        _owner.InsertColumnHeaders([columnHeader]);

        return this;
    }

    public void AddRange(params ListViewColumnHeader[] columnHeaders)
    {
        ArgumentNullException.ThrowIfNull(columnHeaders, nameof(columnHeaders));
        
        _owner.InsertColumnHeaders(columnHeaders);
    }

    public void Clear() => _owner.ClearColumnHeaders();

    public bool Contains(ListViewColumnHeader columnHeader)
    {
        ArgumentNullException.ThrowIfNull(columnHeader, nameof(columnHeader));
        
        return _owner.Contains(columnHeader);
    }

    public IEnumerator<ListViewColumnHeader> GetEnumerator()
    {
        /* Shallow copy the items and return an enumerator off that container. */
        List<ListViewColumnHeader> columnHeaders = new(_owner.ColumnHeaderCount);
        
        for (int i = 0; i < _owner.ColumnHeaderCount; i++) {
            columnHeaders.Add(_owner.GetColumnHeaderByIndex(i));
        }
        
        return columnHeaders.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(ListViewColumnHeader columnHeader)
    {
        ArgumentNullException.ThrowIfNull(columnHeader, nameof(columnHeader));
        
        return _owner.IndexOfColumnHeader(columnHeader);
    }
    
    public ListViewColumnHeader this[int index]
    {
        get {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            
            return _owner.GetColumnHeaderByIndex(index);
        }
        set => throw new InvalidOperationException();
    }
}