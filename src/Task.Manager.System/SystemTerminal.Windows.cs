namespace Task.Manager.System;

public partial class SystemTerminal
{
#if __WIN32__    
    private bool CursorVisibleInternal
    {
        get => Console.CursorVisible;
        set => Console.CursorVisible = value;
    }
#endif
}