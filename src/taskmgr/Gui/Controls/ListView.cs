using System.Drawing;
using System.Text;
using Task.Manager.Configuration;
using Task.Manager.System;

namespace Task.Manager.Gui.Controls;

public class ListView
{
    /* 
     * The Collections act as a proxy for updates to the underlying List<T>.
     * This provides a clean api for interacting with Collections on the ListView
     * control, similar to the Win32 ListView common control. 
     */
    private readonly ListViewColumnHeaderCollection _columnHeaderCollection;
    private readonly ListViewItemCollection _itemCollection;

    /* The containers holding the List<T> for rendering. We don't expose them via a public api. */
    private List<ListViewColumnHeader> _columnHeaders = [];
    private List<ListViewItem> _items = [];

    private ViewPort _viewPort = new();
    
    private ISystemTerminal _terminal;
    
    private Theme _theme;

    /* Buffer for working with strings when writing out terminal content */
    private StringBuilder _buf = new(1024);
    
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

        _buf.Clear();

        /* TODO: Will look into span<T> + stackalloc char[] to fast build strings */
        int c = 0;
        
        for (int i = 0; i < ColumnHeaderCount; i++) {
            if (c + _columnHeaders[i].Width >= _terminal.WindowWidth) {
                break;
            }

            string formatStr = _columnHeaders[i].RightAligned 
                ? "{0," + _columnHeaders[i].Width.ToString() + "}"
                : "{0,-" + _columnHeaders[i].Width.ToString() + "}";
                
            _buf.Append(string.Format(formatStr, _columnHeaders[i].Text));

            if ((i + 1) < ColumnHeaderCount) {
                _buf.Append(' ');
                c++;
            }
            
            c+= _columnHeaders[i].Width;
        }
        
        _terminal.Write(_buf.ToString());
        _terminal.WriteEmptyLineTo(_terminal.WindowWidth - _buf.Length);
    }

    private void DrawItem(
        ListViewItem item,
        int width,
        bool highlight)
    {
        //int nchars = 0;
        
        //for (int i = 0; i < )
    }
    
    private void DrawItems()
    {
        _viewPort.Height = _terminal.WindowHeight - _viewPort.Bounds.Y - 1;
        _viewPort.RowCount = _viewPort.Height - 2;

        int n = 0;

        for (int i = 0; i < _viewPort.RowCount; i++) {
            int pid = i + _viewPort.CurrentIndex;

            if (pid < ItemCount) {
                var item = Items[pid];
                
                DrawItem(
                    item,
                    _viewPort.Bounds.Width,
                    highlight: pid == _viewPort.SelectedIndex);

                n++;
            }
        }

        for (int i = n; i < _viewPort.Height - 1; i++) {
            _terminal.WriteEmptyLine();
        }
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
