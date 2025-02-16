using System.Text;

namespace Task.Manager.System;

public class SystemTerminal : ISystemTerminal
{
    public SystemTerminal()
    {
        Console.OutputEncoding = Encoding.UTF8;
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

    public ConsoleColor ForegroundColor
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }

    public bool KeyAvailable
    {
        get => Console.KeyAvailable;
    }
    
    public TextWriter StdError => Console.Error;
    public TextReader StdIn => Console.In;
    public TextWriter StdOut => Console.Out;
    public void Clear() => Console.Clear();
    public ConsoleKeyInfo ReadKey() => Console.ReadKey();
    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);
    public int WindowWidth => Console.WindowWidth;
    public int WindowHeight => Console.WindowHeight;
    public void Write(char ch) => Console.Write(ch);
    public void Write(string message) => Console.Write(message);
    public void WriteEmptyLine() => WriteEmptyLineTo(Console.WindowWidth);

    public void WriteEmptyLineTo(int x)
    {
        for (int i = 0; i < x; i++) {
            Write('*');
        }
    }
    
    public void WriteLine(char ch) => Console.WriteLine(ch);
    public void WriteLine(string message) => Console.WriteLine(message);   
}