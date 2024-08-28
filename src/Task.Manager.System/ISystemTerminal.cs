namespace Task.Manager.System;

public interface ISystemTerminal
{
    ConsoleColor BackgroundColor { get; set; }
    int CursorLeft { get; set; }
    int CursorTop { get; set; }
    ConsoleColor ForegroundColor { get; set; }
    bool KeyAvailable { get; }
    TextWriter StdError { get; }
    TextReader StdIn { get; }
    TextWriter StdOut { get; }
    void Clear();
    ConsoleKeyInfo ReadKey();
    void SetCursorPosition(int left, int top);
    void Write(char ch);
    void Write(string message);
    void WriteLine(char ch);
    void WriteLine(string message);
}