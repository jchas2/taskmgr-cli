using System.Text;

namespace Task.Manager.System;

public partial class SystemTerminal : ISystemTerminal
{
    public SystemTerminal()
    {
        Console.OutputEncoding = Encoding.UTF8;
        EnableAnsiTerminalCodesInternal();
    }

    public ConsoleColor BackgroundColor
    {
        get => Console.BackgroundColor;
        set => Console.BackgroundColor = value;
    }

    public int CursorLeft
    {
        get => Console.CursorLeft;
        set => Console.CursorLeft = value;
    }

    public int CursorTop
    {
        get => Console.CursorTop;
        set => Console.CursorTop = value;
    }

    public bool CursorVisible
    {
        get => CursorVisibleInternal;
        set => CursorVisibleInternal = value;
    }

    public void EnableAnsiTerminalCodes() => EnableAnsiTerminalCodesInternal();
    
    public ConsoleColor ForegroundColor
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }

    public bool KeyAvailable => Console.KeyAvailable;

    public TextWriter StdError => Console.Error;
    public TextReader StdIn => Console.In;
    public TextWriter StdOut => Console.Out;
    public void Clear() => Console.Clear();
    public ConsoleKeyInfo ReadKey() => Console.ReadKey(intercept: true);
    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);
    public int WindowWidth => Console.WindowWidth;
    public int WindowHeight => Console.WindowHeight;
    public void Write(char ch) => Console.Write(ch);
    public void Write(string message) => Console.Write(message);
    public void WriteEmptyLine() => WriteEmptyLineTo(Console.WindowWidth);

    public void WriteEmptyLineTo(int x)
    {
        if (x <= 0) {
            return;
        }
        Write(new string(' ', x));
    }
    
    public void WriteLine(char ch) => Console.WriteLine(ch);
    public void WriteLine(string message) => Console.WriteLine(message);   
}