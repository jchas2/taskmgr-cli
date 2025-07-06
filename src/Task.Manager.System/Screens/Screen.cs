using Task.Manager.System.Controls;

namespace Task.Manager.System.Screens;

public partial class Screen : Control
{
    public Screen(ISystemTerminal systemTerminal) : base(systemTerminal) { }

    public void Close()
    {
        Unload();
        IsActive = false;
    }

    public bool CursorVisible { get; set; } = true;
    
    private bool IsActive { get; set; } = false;

    public void Show()
    {
        Load();
        Clear();
        Resize();
        Draw();
        IsActive = true;
    }
}
