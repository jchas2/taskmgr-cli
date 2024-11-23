using Task.Manager.System;

namespace Task.Manager.System.Controls;

public class Control(ISystemTerminal terminal)
{
    public ConsoleColor BackgroundColour { get; set; } = ConsoleColor.Black;
    
    public ConsoleColor ForegroundColour { get; set; } = ConsoleColor.White;
   
    protected bool IsActive { get; set; } = false;
    
    protected virtual bool GetInput(int timeoutMilliseconds, ref ConsoleKeyInfo keyInfo)
    {
        const int waitMilliseconds = 50;
        DateTime timeout = DateTime.Now.AddMilliseconds(timeoutMilliseconds);

        while (DateTime.Now < timeout) {
            
            if (terminal.KeyAvailable) {
                keyInfo = terminal.ReadKey();
                return true;
            }

            Thread.Sleep(waitMilliseconds); 
        }

        return false;
    }
    
    protected ISystemTerminal Terminal { get => terminal; }
}
