using Task.Manager.System;

namespace Task.Manager.System.Controls;

public class Control(ISystemTerminal terminal)
{
    public ConsoleColor BackgroundColour { get; set; } = ConsoleColor.Black;
    
    public ConsoleColor ForegroundColour { get; set; } = ConsoleColor.White;
   
    protected bool IsActive { get; set; } = false;
    
    protected virtual bool GetInput(int timeoutMilliseconds, ref ConsoleKeyInfo keyInfo)
    {
        if (false == terminal.KeyAvailable) {
            return false;
        }

        keyInfo = terminal.ReadKey();
        return true;
    }
}
