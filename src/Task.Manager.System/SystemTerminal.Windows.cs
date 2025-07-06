namespace Task.Manager.System;

public partial class SystemTerminal
{
#if __WIN32__    
    private bool CursorVisibleInternal
    {
        // TODO:
        get => true;
        set { }
    }
#endif
}