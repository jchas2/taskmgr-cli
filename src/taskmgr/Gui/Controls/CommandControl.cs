using System.Text;
using Task.Manager.Cli.Utils;
using Task.Manager.Commands;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;

namespace Task.Manager.Gui.Controls;

public sealed class CommandControl(ISystemTerminal terminal, AppConfig appConfig) : Control(terminal)
{
    private const int CommandLength = 10;

    private readonly Dictionary<ConsoleKey, AbstractCommand> commandMap = new();

    public CommandControl AddCommand(ConsoleKey key, Func<AbstractCommand> commandFactory)
    {
        commandMap.Add(key, commandFactory.Invoke());
        return this;
    }
    
    protected override void OnDraw()
    {
        try {
            Control.DrawingLockAcquire(); 
            OnDrawInternal();
        }
        finally {
            Control.DrawingLockRelease();
        }
    }
    
    private void OnDrawInternal()
    {
        using TerminalColourRestorer _ = new();
        
        Terminal.SetCursorPosition(left: X, top: Y);
        int nchars = 0;
        
        foreach (ConsoleKey key in commandMap.Keys) {
            AbstractCommand cmd = commandMap[key];
            
            string commandText = cmd.Text.Length > CommandLength
                ? cmd.Text.Substring(0, CommandLength - 1)
                : cmd.Text.PadRight(CommandLength);
            
            nchars += KeyBindControl.Draw(
                key.ToString(),
                commandText,
                nchars,
                Y,
                CommandLength,
                appConfig.DefaultTheme,
                cmd.IsEnabled,
                Terminal);
        }
        
        Terminal.WriteEmptyLineTo(Width - nchars);
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        if (!commandMap.TryGetValue(keyInfo.Key, out var cmd)) {
            return;
        }

        if (cmd.IsEnabled) {
            cmd.Execute();
        }
        
        handled = true;            
    }
}
