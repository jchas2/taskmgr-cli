using Task.Manager.System;

namespace Task.Manager.System.Controls;

public class Control(ISystemTerminal terminal)
{
    public ConsoleColor BackgroundColour { get; set; } = ConsoleColor.Black;
    
    public ConsoleColor ForegroundColour { get; set; } = ConsoleColor.White;
   
    protected bool IsActive { get; set; } = false;
    
    protected virtual bool GetInput(int timeoutMilliseconds, ref ConsoleKeyInfo keyInfo)
    {
        const int waitMilliseconds = 100;
        DateTime start = DateTime.Now;

        while (true) {
            if (DateTime.Now.Subtract(start).TotalMilliseconds < timeoutMilliseconds) {
                if (terminal.KeyAvailable) {
                    keyInfo = terminal.ReadKey();
                    return true;
                }
                if (timeoutMilliseconds > waitMilliseconds * 2) {
                    Thread.Sleep(waitMilliseconds);
                }
                continue;
            }
            break;
        }

        return false;
    }
}
