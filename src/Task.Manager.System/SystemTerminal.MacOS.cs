namespace Task.Manager.System;

public partial class SystemTerminal : ISystemTerminal
{
    
    private const string AnsiHideCursor = "\x1b[?25l";
    private const string AnsiShowCursor = "\x1b[?25h";
    
#if __APPLE__

#endif
}