using Task.Manager.System;

namespace Task.Manager.System.Controls;

public class Control
{
    /*
     * The Collection acts as a proxy for updates to the underlying List<T>.
     * This provides a clean api for interacting with the Collection on the
     * control, similar to the WinForms Controls Collection.
     */
    private readonly ControlCollection _controlCollection;

    /* The container holding the List<T> for rendering. We don't expose it via a public api. */
    private List<Control> _controls = [];

    private static readonly object _drawingLock = new();
    private static int _drawingLocksAcquired = 0;
    
    private readonly ISystemTerminal _terminal;

    public Control(ISystemTerminal terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _controlCollection = new ControlCollection(this);
    }

    public static void DrawingLockAcquire()
    {
        _drawingLocksAcquired++;
        
        Monitor.Enter(_drawingLock);
    }

    public static void DrawingLockRelease()
    {
        if (_drawingLocksAcquired == 0) {
            return;
        }
        
        _drawingLocksAcquired--;
        
        Monitor.Exit(_drawingLock);
    }
    
    public ConsoleColor BackgroundColour { get; set; } = ConsoleColor.Black;

    public void Clear() => OnClear();
    
    internal void ClearControls() => _controls.Clear();

    public void Unload() => OnUnload();
    
    public void Draw()
    {
        if (Visible) {
            OnDraw();
        }
    }

    internal int ControlCount => _controls.Count;
    
    public ConsoleColor ForegroundColour { get; set; } = ConsoleColor.White;
    
    internal bool ContainsControl(Control control)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));
        
        return _controls.Contains(control);
    }
   
    public ControlCollection Controls => _controlCollection;

    internal Control GetControlByIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _controls.Count, nameof(index));
        
        return _controls[index];
    }
    
    public int Height { get; set; } = 0;

    internal int IndexOfControl(Control control)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));

        for (int i = 0; i < _controls.Count; i++) {
            if (_controls[i] == control) {
                return i;
            }
        }

        return -1;
    }
    
    internal void InsertControl(int index, Control control)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _controls.Count, nameof(index));
        
        _controls.Insert(index, control);
    }

    internal void InsertControls(Control[] controls)
    {
        ArgumentNullException.ThrowIfNull(controls, nameof(controls));
        
        _controls.AddRange(controls);
    }
    
    public void KeyPressed(ConsoleKeyInfo keyInfo, ref bool handled) => OnKeyPressed(keyInfo, ref handled);
    
    public void Load() => OnLoad();
    
    protected virtual void OnClear()
    {
        for (int i = Y; i < Height; i++) {
            _terminal.SetCursorPosition(X, i);
            _terminal.WriteEmptyLineTo(Width);
        }
    }

    protected virtual void OnDraw() { }

    protected virtual void OnLoad() { }

    protected virtual void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled) { }

    protected virtual void OnResize() { }

    protected virtual void OnUnload()
    {
        foreach (Control control in Controls) {
            control.Unload();
        }
    }
    
    internal void RemoveControlAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _controls.Count, nameof(index));
        
        _controls.RemoveAt(index);
    }

    internal void RemoveControl(Control control)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));
        
        int index = IndexOfControl(control);
        
        if (index != -1) {
            RemoveControlAt(index);
        }
    }

    public void Resize()
    {
        if (Visible) {
            OnResize();
        }
    }

    protected ISystemTerminal Terminal => _terminal;
    
    public bool Visible { get; set; } = true;
    
    public int Width { get; set; } = 0;

    public int X { get; set; } = 0;
    
    public int Y { get; set; } = 0;
}
