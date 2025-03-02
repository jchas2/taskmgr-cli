﻿using System.Drawing;
using System.Text;
using Task.Manager.Cli.Utils;

namespace Task.Manager.System.Controls.ListView;

public class ListView : Control
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
    
    /* Buffer for working with strings when writing out terminal content */
    private StringBuilder _buffer = new(1024);

    private const int DefaultColumnWidth = 30;
    private const int DefaultHeaderWidth = 80;

    public ListView(ISystemTerminal terminal)
        : base(terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _itemCollection = new ListViewItemCollection(this);
        _columnHeaderCollection = new ListViewColumnHeaderCollection(this);
    }
    
    public ConsoleColor BackgroundHighlightColour { get; set; } = ConsoleColor.White;

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

        if (ColumnHeaderCount == 0) {
            _terminal.WriteEmptyLine();
            return;
        }

        _buffer.Clear();

        /* TODO: Will look into span<T> + stackalloc char[] to fast build strings */
        int c = 0;
        
        for (int i = 0; i < ColumnHeaderCount; i++) {
            if (c + _columnHeaders[i].Width >= _terminal.WindowWidth) {
                break;
            }

            string formatStr = _columnHeaders[i].RightAligned 
                ? "{0," + _columnHeaders[i].Width.ToString() + "}"
                : "{0,-" + _columnHeaders[i].Width.ToString() + "}";

            _buffer.Append((string.Format(formatStr, _columnHeaders[i].Text) + ' ')
                .ToColour(_columnHeaders[i].ForegroundColour, _columnHeaders[i].BackgroundColour));
            
            c+= _columnHeaders[i].Width + 1;
        }
        
        _terminal.Write(_buffer.ToString());
        _terminal.WriteEmptyLineTo(_terminal.WindowWidth - c);
    }

    private void DrawItem(
        ListViewItem item,
        int width,
        bool highlight)
    {
        _buffer.Clear();

        /* TODO: Will look into span<T> + stackalloc char[] to fast build strings */
        int c = 0;
        
        for (int i = 0; i < item.SubItemCount; i++) {
            var subItem = item.SubItems[i];
            
            bool rightAligned = false;
            int columnWidth = DefaultColumnWidth;

            if (i < ColumnHeaderCount) {
                rightAligned = _columnHeaders[i].RightAligned;
                columnWidth = _columnHeaders[i].Width;
            }

            if (c + columnWidth >= _terminal.WindowWidth) {
                break;
            }
            
            string formatStr = rightAligned 
                ? "{0," + columnWidth.ToString() + "}"
                : "{0,-" + columnWidth.ToString() + "}";

            ConsoleColor foregroundColour = highlight
                ? ForegroundHighlightColour
                : subItem.ForegroundColor;
            
            ConsoleColor backgroundColour = highlight
                ? BackgroundHighlightColour
                : subItem.BackgroundColor;

            if (subItem.Text.Length > columnWidth) {
                _buffer.Append((string.Format(formatStr, subItem.Text.Substring(0, columnWidth)) + ' ')
                    .ToColour(foregroundColour, backgroundColour));
            }
            else {
                _buffer.Append((string.Format(formatStr, subItem.Text) + ' ')
                    .ToColour(foregroundColour, backgroundColour));
            }
            
            c += columnWidth + 1;
        }

        _terminal.Write(_buffer.ToString());
        _terminal.WriteEmptyLineTo(_terminal.WindowWidth - c);
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

    public ConsoleColor ForegroundHighlightColour { get; set; } = ConsoleColor.Cyan;

    internal ListViewColumnHeader GetColumnHeaderByIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _columnHeaders.Count, nameof(index));
        return _columnHeaders[index];
    }
    
    internal ListViewItem GetItemByIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _items.Count, nameof(index));
        return _items[index];
    }

    public ConsoleColor HeaderBackgroundColour { get; set; } = ConsoleColor.Black;
    
    public ConsoleColor HeaderForegroundColour { get; set; } = ConsoleColor.White;
    
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
