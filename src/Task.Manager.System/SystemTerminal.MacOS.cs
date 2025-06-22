namespace Task.Manager.System;

public partial class SystemTerminal : ISystemTerminal
{
#if __APPLE__
    private const string AnsiHideCursor = "\x1b[?25l";
    private const string AnsiShowCursor = "\x1b[?25h";
    
    // TODO:
    private bool CursorVisibleInternal
    {
        get => true;
        set { }
    }
#endif
}