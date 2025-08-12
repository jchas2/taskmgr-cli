using Task.Manager.Cli.Utils;
using Task.Manager.System;

namespace Task.Manager.System.Controls;

public class Control
{
    /*
     * The Collection acts as a proxy for updates to the underlying List<T>.
     * This provides a clean api for interacting with the Collection on the
     * control, similar to the WinForms Controls Collection.
     */
    private readonly ControlCollection controlCollection;

    /* The container holding the List<T> for rendering. We don't expose it via a public api. */
    private List<Control> controls = [];

    private static readonly object drawingLock = new();
    private static int drawingLocksAcquired = 0;
    
    private readonly ISystemTerminal terminal;

    public Control(ISystemTerminal terminal)
    {
        this.terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        controlCollection = new ControlCollection(this);
    }

    public ConsoleColor BackgroundColour { get; set; } = ConsoleColor.Black;

    public void Clear() => OnClear();
    
    internal void ClearControls() => controls.Clear();

    protected static void DrawingLockAcquire()
    {
        drawingLocksAcquired++;
        
        Monitor.Enter(drawingLock);
    }

    protected static void DrawingLockRelease()
    {
        if (drawingLocksAcquired == 0) {
            return;
        }
        
        drawingLocksAcquired--;
        
        Monitor.Exit(drawingLock);
    }

    protected void DrawRectangle(
        int x, 
        int y, 
        int width, 
        int height, 
        ConsoleColor colour)
    {
        using TerminalColourRestorer _ = new();
        Terminal.BackgroundColor = colour;
        
        for (int i = y; i < y + height; i++) {
            terminal.SetCursorPosition(x, i);
            terminal.WriteEmptyLineTo(width);
        }
    }

    public void Unload() => OnUnload();
    
    public void Draw()
    {
        if (RedrawEnabled && Visible) {
            OnDraw();
        }
    }

    internal int ControlCount => controls.Count;
    
    public ConsoleColor ForegroundColour { get; set; } = ConsoleColor.White;
    
    internal bool ContainsControl(Control control)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));
        
        return controls.Contains(control);
    }
   
    public ControlCollection Controls => controlCollection;

    internal Control GetControlByIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, controls.Count, nameof(index));
        
        return controls[index];
    }
    
    public int Height { get; set; } = 0;

    internal int IndexOfControl(Control control)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));

        for (int i = 0; i < controls.Count; i++) {
            if (controls[i] == control) {
                return i;
            }
        }

        return -1;
    }
    
    internal void InsertControl(int index, Control control)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, controls.Count, nameof(index));
        
        controls.Insert(index, control);
    }

    internal void InsertControls(Control[] controls)
    {
        ArgumentNullException.ThrowIfNull(controls, nameof(controls));
        
        this.controls.AddRange(controls);
    }
    
    public void KeyPressed(ConsoleKeyInfo keyInfo, ref bool handled) => OnKeyPressed(keyInfo, ref handled);
    
    public void Load() => OnLoad();
    
    protected virtual void OnClear() =>
        DrawRectangle(
            X, 
            Y, 
            Width, 
            Height, 
            BackgroundColour);

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

    public static bool RedrawEnabled { get; set; } = true;
    
    internal void RemoveControlAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, controls.Count, nameof(index));
        
        controls.RemoveAt(index);
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

    protected ISystemTerminal Terminal => terminal;
    
    public bool Visible { get; set; } = true;
    
    public int Width { get; set; } = 0;

    public int X { get; set; } = 0;
    
    public int Y { get; set; } = 0;
}
