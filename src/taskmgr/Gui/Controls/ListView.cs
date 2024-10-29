using System.Drawing;
using System.Text;
using Task.Manager.Configuration;
using Task.Manager.System;

namespace Task.Manager.Gui.Controls;

public class ListView
{
    /* 
     * The Collections acts as a proxy for updates to the underlying List<T>.
     * This provides a clean api for interacting with an Collections on the ListView
     * control, similar to the old Win32 ListView common control. 
     */
    private readonly ListViewColumnHeaderCollection _columnHeaderCollection;
    private readonly ListViewItemCollection _itemCollection;

    /* The containers holding the List<T> for rendering. We don't expose them via a public api. */
    private List<ListViewColumnHeader> _columnHeaders = [];
    private List<ListViewItem> _items = [];

    private ViewPort _viewPort = new();
    
    private ISystemTerminal _terminal;
    
    private Theme _theme;

    private const int DefaultHeaderWidth = 80;

    public ListView(ISystemTerminal terminal, Theme theme)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _itemCollection = new ListViewItemCollection(this);
        _columnHeaderCollection = new ListViewColumnHeaderCollection(this);
    }
    
    public ConsoleColor BackgroundColour { get; set; }

    internal void ClearColumnHeaders() => _columnHeaders.Clear();
    
    internal void ClearItems() => _items.Clear();

    internal int ColumnHeaderCount => _columnHeaders.Count;

    public ListViewColumnHeaderCollection ColumnHeaders => _columnHeaderCollection;
    
    internal bool Contains(ListViewColumnHeader columnHeader)
    {
        ArgumentNullException.ThrowIfNull(columnHeader, nameof(columnHeader));
        return _columnHeaders.Contains(columnHeader);
    }
    
    internal bool Contains(ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        return _items.Contains(item);
    }

    public void Draw(in Rectangle bounds)
    {
        _viewPort.Bounds = bounds;
        
        DrawHeader();
        DrawItems();
    }

    private void DrawHeader()
    {
        _terminal.SetCursorPosition(_viewPort.Bounds.X, _viewPort.Bounds.Y - 1);
        _terminal.BackgroundColor = _theme.HeaderBackground;
        _terminal.ForegroundColor = _theme.HeaderForeground;

        if (ColumnHeaderCount == 0) {
            _terminal.WriteEmptyLine();
            return;
        }

        /* TODO: This code is terrible, and will perform like a dog. */
        /* Will look into span<T> + stackalloc char[] to fast build strings */
        int c = 0;
        for (int i = 0; i < ColumnHeaderCount; i++) {
            if (c + _columnHeaders[i].Width >= _terminal.WindowWidth) {
                break;
            }
            
            string formatString = "{0,";
            formatString += _columnHeaders[i].RightAligned
                ? _columnHeaders[i].Width.ToString() + "}"
                : "-" + _columnHeaders[i].Width.ToString() + "}";
            
            _terminal.Write(string.Format(formatString, _columnHeaders[i].Text));
            
            c+= _columnHeaders[i].Width;
        }
    }

    private void DrawItem()
    {
    }
    
    private void DrawItems()
    {
    }
    
    public ConsoleColor ForegroundColour { get; set; }

    internal ListViewColumnHeader GetColumnHeaderByIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Count, nameof(index));
        return _columnHeaders[index];
    }
    
    internal ListViewItem GetItemByIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Count, nameof(index));
        return _items[index];
    }

    internal int IndexOfColumnHeader(ListViewColumnHeader columnHeader)
    {
        ArgumentNullException.ThrowIfNull(columnHeader, nameof(columnHeader));

        for (int i = 0; i < _columnHeaders.Count; i++) {
            if (_columnHeaders[i] == columnHeader) {
                return i;
            }
        }

        return -1;
    }
    
    internal int IndexOfItem(ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        for (int i = 0; i < _items.Count; i++) {
            if (_items[i] == item) {
                return i;
            }
        }

        return -1;
    }

    internal void InsertColumnHeader(int index, ListViewColumnHeader columnHeader)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentNullException.ThrowIfNull(columnHeader, nameof(columnHeader));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Count, nameof(index));
        _columnHeaders.Insert(index, columnHeader);
    }

    internal void InsertColumnHeaders(ListViewColumnHeader[] columnHeaders)
    {
        ArgumentNullException.ThrowIfNull(columnHeaders, nameof(columnHeaders));
        _columnHeaders.AddRange(columnHeaders);
    }
    
    internal void InsertItem(int index, ListViewItem item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _items.Count, nameof(index));
        _items.Insert(index, item);
    }

    internal void InsertItems(ListViewItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));
        _items.AddRange(items);
    }
    
    internal int ItemCount => _items.Count;
    
    public ListViewItemCollection Items => _itemCollection;

    internal void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Count, nameof(index));
        _items.RemoveAt(index);
    }

    internal void RemoveItem(ListViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        int index = IndexOfItem(item);
        
        if (index != -1) {
            RemoveAt(index);
        }
    }
}
