namespace Task.Manager.System;

public sealed class OutputWriter
{
    private readonly TextWriter _writer;

    private static readonly OutputWriter _errorWriter = new OutputWriter(Console.Error);
    private static readonly OutputWriter _outWriter = new OutputWriter(Console.Out);
    
    public OutputWriter(TextWriter writer) => 
        _writer = writer;
    
    public static OutputWriter Error => _errorWriter;

    public static OutputWriter Out => _outWriter;
    
    public void Write(string message) =>
        _writer?.Write(message);

    public void WriteLine() =>
        _writer?.WriteLine();

    public void WriteLine(string message) =>
        _writer?.WriteLine(message);
    
    public void WriteLine(string format, params object?[] args) =>
        _writer?.WriteLine(string.Format(format, args));
}
