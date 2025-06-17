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
    
    private readonly ISystemTerminal _terminal;
    
    public Control(ISystemTerminal terminal)
    {
        _terminal = terminal;
        _controlCollection = new ControlCollection(this);
    }

    public ConsoleColor BackgroundColour { get; set; } = ConsoleColor.Black;
    
    internal void ClearControls() => _controls.Clear();

    public void Close() => OnUnload();
    
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

    private bool IsActive { get; set; } = false;
    
    public virtual bool GetInput(ref ConsoleKeyInfo keyInfo)
    {
        if (_terminal.KeyAvailable) {
            keyInfo = _terminal.ReadKey();
            return true;
        }

        return false;
    }

    public int Height { get; set; } = 0;
    
    protected virtual void OnLoad() => IsActive = true;

    protected virtual void OnUnload()
    {
        foreach (var control in Controls) {
            control.Close();
        }
        
        IsActive = false;   
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

    public void Show() => OnLoad();
    
    protected ISystemTerminal Terminal => _terminal;
    
    public int Width { get; set; } = 0;

    public int X { get; set; } = 0;
    
    public int Y { get; set; } = 0;
}
