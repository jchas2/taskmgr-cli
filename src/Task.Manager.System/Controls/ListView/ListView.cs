using System.Diagnostics;
using System.Drawing;
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

    public event EventHandler<ListViewItemEventArgs>? ItemClicked;
    public event EventHandler<ListViewItemEventArgs>? ItemSelected;

    public ListView(ISystemTerminal terminal)
        : base(terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _itemCollection = new ListViewItemCollection(this);
        _columnHeaderCollection = new ListViewColumnHeaderCollection(this);

        EnableRowSelect = true;
        EnableScroll = true;
        ShowColumnHeaders = true; 
    }
    
    public ConsoleColor BackgroundHighlightColour { get; set; } = ConsoleColor.White;

    internal void ClearColumnHeaders() => _columnHeaders.Clear();

    internal void ClearItems()
    {
        _items.Clear();
        _viewPort.Reset();
    }

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
    
    private void DoScroll(ConsoleKey consoleKey, out bool redrawAllItems)
    {
        redrawAllItems = false;
        
        switch (consoleKey) {
            case ConsoleKey.DownArrow:
                if (_viewPort.SelectedIndex != _items.Count - 1) {
                    _viewPort.PreviousSelectedIndex = _viewPort.SelectedIndex;
                    _viewPort.SelectedIndex++;
                    
                    if (_viewPort.SelectedIndex - _viewPort.CurrentIndex >= _viewPort.RowCount) {
                        if (_viewPort.CurrentIndex <= _items.Count - _viewPort.Height + 1) {
                            _viewPort.CurrentIndex++;
                            redrawAllItems = true;
                        }
                    }
                }
                break;
            
            case ConsoleKey.UpArrow:
                if (_viewPort.SelectedIndex != 0) {
                    _viewPort.PreviousSelectedIndex = _viewPort.SelectedIndex;
                    _viewPort.SelectedIndex--;
                    
                    if (_viewPort.SelectedIndex <= _viewPort.CurrentIndex - 1 && _viewPort.CurrentIndex != 0) {
                        _viewPort.CurrentIndex--;
                        redrawAllItems = true;
                    }
                }
                else {
                    _viewPort.PreviousSelectedIndex = _viewPort.SelectedIndex;
                }
                break;
            
            case ConsoleKey.PageDown:
                if (_viewPort.SelectedIndex != _items.Count - 1) {
                    _viewPort.PreviousSelectedIndex = _viewPort.SelectedIndex;
                    _viewPort.SelectedIndex += _viewPort.RowCount;
                    
                    if (_viewPort.SelectedIndex > _items.Count - 1) {
                        _viewPort.SelectedIndex = _items.Count - 1;
                    }
                    
                    if (_viewPort.SelectedIndex - _viewPort.CurrentIndex >= _viewPort.RowCount) {
                        _viewPort.CurrentIndex = Math.Min(_items.Count - _viewPort.RowCount, _viewPort.SelectedIndex);
                        redrawAllItems = true;
                    }
                }
                break;
            
            case ConsoleKey.PageUp:
                if (_viewPort.SelectedIndex != 0) {
                    _viewPort.PreviousSelectedIndex = _viewPort.SelectedIndex;
                    
                    if (_viewPort.SelectedIndex > _viewPort.RowCount) {
                        _viewPort.SelectedIndex -= _viewPort.RowCount;
                    }
                    else {
                        _viewPort.SelectedIndex = 0;
                    }
                    
                    if (_viewPort.SelectedIndex <= _viewPort.CurrentIndex - 1 && _viewPort.CurrentIndex != 0) {
                        _viewPort.CurrentIndex = Math.Max(0, _viewPort.SelectedIndex);
                        redrawAllItems = true;
                    }
                }
                break;
        }
    }
    
    private void DrawHeader()
    {
        using TerminalColourRestorer _ = new();

        _terminal.SetCursorPosition(_viewPort.Bounds.X, _viewPort.Bounds.Y - 1);

        if (ColumnHeaderCount == 0) {
            _terminal.WriteEmptyLineTo(_viewPort.Bounds.Width);
            return;
        }

        _buffer.Clear();

        /* TODO: Will look into span<T> + stackalloc char[] to fast build strings */
        int c = 0;
        
        for (int i = 0; i < ColumnHeaderCount; i++) {
            if (c + _columnHeaders[i].Width > _viewPort.Bounds.Width) {
                break;
            }

            int columnHeaderFormatWidth = _columnHeaders[i].Width > 0 
                ? _columnHeaders[i].Width - 1
                : _columnHeaders[i].Width;
            
            string formatStr = _columnHeaders[i].RightAligned 
                ? "{0," + columnHeaderFormatWidth.ToString() + "}"
                : "{0,-" + columnHeaderFormatWidth.ToString() + "}";

            ConsoleColor foreground = _columnHeaders[i].ForegroundColour ?? HeaderForegroundColour;
            ConsoleColor background = _columnHeaders[i].BackgroundColour ?? HeaderBackgroundColour;

            if (_columnHeaders[i].Text.Length > _columnHeaders[i].Width) {
                _buffer.Append((string.Format(formatStr, _columnHeaders[i].Text.Substring(0, _columnHeaders[i].Width - 1)) + ' ')
                    .ToColour(foreground, background));
            }
            else {
                _buffer.Append((string.Format(formatStr, _columnHeaders[i].Text) + ' ')
                    .ToColour(foreground, background));
            }

            c+= _columnHeaders[i].Width;
        }
        
        _terminal.Write(_buffer.ToString());
        _terminal.BackgroundColor = HeaderBackgroundColour;
        _terminal.WriteEmptyLineTo(_viewPort.Bounds.Width - c);
    }

    private void DrawItem(
        ListViewItem item,
        int width,
        bool highlight)
    {
        using TerminalColourRestorer _ = new();

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

            if (c + columnWidth > _viewPort.Bounds.Width) {
                break;
            }

            int columnFormatWidth = columnWidth > 0
                ? columnWidth - 1
                : columnWidth;

            string formatStr = rightAligned
                ? "{0," + columnFormatWidth.ToString() + "}"
                : "{0,-" + columnFormatWidth.ToString() + "}";

            ConsoleColor foregroundColour = highlight
                ? EnableRowSelect
                    ? ForegroundHighlightColour
                    : subItem.ForegroundColor
                : subItem.ForegroundColor;

            ConsoleColor backgroundColour = highlight
                ? EnableRowSelect
                    ? BackgroundHighlightColour
                    : subItem.BackgroundColor
                : subItem.BackgroundColor;

            if (subItem.Text.Length > columnWidth) {
                _buffer.Append((string.Format(formatStr, subItem.Text.Substring(0, columnWidth - 1)) + ' ')
                    .ToColour(foregroundColour, backgroundColour));
            }
            else {
                _buffer.Append((string.Format(formatStr, subItem.Text) + ' ')
                    .ToColour(foregroundColour, backgroundColour));
            }

            c += columnWidth;
        }

        _terminal.Write(_buffer.ToString());
        _terminal.BackgroundColor = item.SubItems[item.SubItemCount - 1].BackgroundColor;
        _terminal.WriteEmptyLineTo(_viewPort.Bounds.Width - c);
    }

    private void DrawItems()
    {
        _viewPort.Height = _viewPort.Bounds.Height;
        _viewPort.RowCount = _viewPort.Height - 1;

        int n = 0;

        for (int i = 0; i < _viewPort.RowCount; i++) {
            int pid = i + _viewPort.CurrentIndex;

            if (pid < ItemCount) {
                var item = Items[pid];
                
                _terminal.SetCursorPosition(_viewPort.Bounds.X, _viewPort.Bounds.Y + n);
                
                DrawItem(
                    item,
                    _viewPort.Bounds.Width,
                    highlight: pid == _viewPort.SelectedIndex);

                n++;
            }
        }

        _terminal.BackgroundColor = BackgroundColour;

        for (int i = n; i < Height - 1; i++) {
            _terminal.SetCursorPosition(_viewPort.Bounds.X, _viewPort.Bounds.Y + i);
            _terminal.WriteEmptyLineTo(_viewPort.Bounds.Width);
        }
    }

    public bool EnableRowSelect { get; set; }
    
    public bool EnableScroll { get; set; }
    
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
    
    protected override void OnDraw()
    {
        // Bounds is the scrollable region for the ListViewItems. The value 1
        // is added to make room for the header.
        int y = ShowColumnHeaders 
            ? Y + 1 
            : Y;
        
        _viewPort.Bounds = new Rectangle(X, y, Width, Height);

        if (ShowColumnHeaders) {
            DrawHeader();
        }
        
        DrawItems();
    }

    protected void OnItemClicked(ListViewItem item) =>
        ItemClicked?.Invoke(this, new ListViewItemEventArgs(item));

    protected void OnItemSelected(ListViewItem item) =>
        ItemSelected?.Invoke(this, new ListViewItemEventArgs(item));

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        if (Items.Count == 0) {
            return;
        }
        
        switch (keyInfo.Key) {
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow:
            case ConsoleKey.PageUp:
            case ConsoleKey.PageDown: {
                if (!EnableScroll) {
                    return;
                }
                
                DoScroll(keyInfo.Key, out bool redrawAllItems);

                if (redrawAllItems) {
                    DrawHeader();
                    DrawItems();
                }
                else {
                    RedrawItem();
                }

                if (SelectedIndex != -1) {
                    OnItemClicked(SelectedItem!);
                }
                
                handled = true;
                break;
            }
            case ConsoleKey.Enter: {
                
                if (SelectedIndex != -1) {
                    OnItemSelected(SelectedItem!);
                }
                
                handled = true;
                break;
            }
        }
    }

    private void RedrawItem()
    {
        _terminal.SetCursorPosition(X, _viewPort.Bounds.Y + _viewPort.SelectedIndex - _viewPort.CurrentIndex);
                    
        var selectedItem = _items[_viewPort.SelectedIndex];

        DrawItem(
            selectedItem,
            _viewPort.Bounds.Width,
            highlight: true);

        if (_viewPort.PreviousSelectedIndex != _viewPort.SelectedIndex) {
            _terminal.SetCursorPosition(X, _viewPort.Bounds.Y + _viewPort.PreviousSelectedIndex - _viewPort.CurrentIndex);
                        
            var previousSelectedItem = _items[_viewPort.PreviousSelectedIndex];

            DrawItem(
                previousSelectedItem,
                _viewPort.Bounds.Width,
                highlight: false);
        }
    }
    
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

    public int SelectedIndex
    {
        get => _viewPort.SelectedIndex;
        set {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, _items.Count, nameof(value));

            _viewPort.SelectedIndex = value;
        }
    }

    public ListViewItem? SelectedItem
    {
        get {
            if (_items.Count == 0) {
                return null;
            }
            
            return _items[SelectedIndex];
        }
    } 
    
    public bool ShowColumnHeaders { get; set; }
}
