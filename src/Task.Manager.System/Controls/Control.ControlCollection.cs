using System.Collections;

namespace Task.Manager.System.Controls;

public class ControlCollection : IEnumerable<Control>
{
    private readonly Control _owner;
    
    public ControlCollection(Control owner) =>
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    
    public void Add(Control control)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));
        
        _owner.InsertControls([control]);
    }

    public void AddRange(params Control[] controls)
    {
        ArgumentNullException.ThrowIfNull(controls, nameof(controls));
        
        _owner.InsertControls(controls);
    }

    public void Clear() => _owner.ClearControls();

    public bool Contains(Control control)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));
        
        return _owner.ContainsControl(control);
    }

    public int Count => _owner.ControlCount;

    public IEnumerator<Control> GetEnumerator()
    {
        /* Shallow copy the items and return an enumerator off that container. */
        List<Control> items = new(_owner.Controls.Count);
        
        for (int i = 0; i < _owner.ControlCount; i++) {
            items.Add(_owner.GetControlByIndex(i));
        }
        
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(Control control)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));
        
        return _owner.IndexOfControl(control);
    }

    public void Remove(Control control)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));
        
        _owner.RemoveControl(control);
    }
    
    public Control this[int index]
    {
        get {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            
            return _owner.GetControlByIndex(index);
        }
        set {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            
            _owner.RemoveControlAt(index);
            _owner.InsertControl(index, value);
        }
    }
}