using Task.Manager.Cli.Utils;

namespace Task.Manager.System.Controls.InputBox;

public sealed class InputBox(ISystemTerminal terminal) : Control(terminal)
{
    private const int MinWidth = 40;
    private const int MinHeight = 1;

    private readonly TextBuffer textBuffer = new();
    
    // TODO: Theme.
    private readonly ConsoleColor boxColour = ConsoleColor.Gray;

    protected override void OnDraw()
    {
        if (Width < MinWidth || Height < MinHeight) {
            return;
        }
    
        using TerminalColourRestorer _ = new();

        DrawRectangle(
            X,
            Y,
            Width,
            Height,
            boxColour);
         
        Terminal.SetCursorPosition(X, Y);

        if (!string.IsNullOrEmpty(Text)) {
            Terminal.BackgroundColor = boxColour;
            Terminal.ForegroundColor = ConsoleColor.Black;
            Terminal.Write(Text);
        }
        
        Terminal.CursorVisible = true;
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        Result = InputBoxResult.None;
        handled = true;

        using TerminalColourRestorer _ = new();

        Terminal.BackgroundColor = boxColour;
        Terminal.ForegroundColor = ConsoleColor.Black;
        
        switch (keyInfo.Key) {
            case ConsoleKey.Enter:
                Result = InputBoxResult.Enter;
                break;
            
            case ConsoleKey.Escape:
                textBuffer.Clear();
                Result = InputBoxResult.Cancel;
                break;
            
            case ConsoleKey.Backspace:
                if (textBuffer.MoveBackwards()) {
                    Terminal.CursorLeft--;
                    Terminal.Write(textBuffer.Text.Substring(textBuffer.CursorBufferPosition) + " ");
                    Terminal.CursorLeft -= textBuffer.Text.Length - textBuffer.CursorBufferPosition + 1;
                }
                break;
            
            case ConsoleKey.Delete:
                if (textBuffer.Delete()) {
                    Terminal.Write(textBuffer.Text.Substring(textBuffer.CursorBufferPosition) + " ");
                    Terminal.CursorLeft -= textBuffer.Length - textBuffer.CursorBufferPosition + 1;
                }
                break;
            
            case ConsoleKey.LeftArrow:
                if (textBuffer.MoveLeft()) {
                    Terminal.CursorLeft--;
                }
                break;
            
            case ConsoleKey.RightArrow:
                if (textBuffer.MoveRight()) {
                    Terminal.CursorLeft++;
                }
                break;
            
            case ConsoleKey.Insert:
                textBuffer.InsertMode = !textBuffer.InsertMode;
                break;
            
            default:
                if (!textBuffer.Add(keyInfo.KeyChar)) {
                    break;
                }

                if (textBuffer.InsertMode) {
                    int currentCursorPosition = Terminal.CursorLeft;
                    Terminal.Write(textBuffer.Text.Substring(textBuffer.CursorBufferPosition - 1));
                    Terminal.CursorLeft = currentCursorPosition + 1;
                }
                else {
                    Terminal.Write(keyInfo.KeyChar);
                }

                break;
        }
    }

    public InputBoxResult Result { get; private set; } = InputBoxResult.Enter;

    public void ShowInputBox()
    {
        OnResize();
        OnDraw();
    }

    public string Text => textBuffer.Text;
    
    public string Title { get; set; } = string.Empty;
}





// ***
    // private const int MinWidth = 40;
    // private const int MinHeight = 1;
    //
    // private StringBuilder _buffer = new();
    // private int _cursorBufferPosition = 0;
    // private bool _bufferInsertMode = true;
    //
    // // TODO: Theme.
    // private ConsoleColor _boxColour = ConsoleColor.Gray;
    //
    // public InputBox(ISystemTerminal terminal) : base(terminal) { }
    //
    // protected override void OnDraw()
    // {
    //     if (Width < MinWidth || Height < MinHeight) {
    //         return;
    //     }
    //
    //     using TerminalColourRestorer _ = new();
    //
    //     DrawRectangle(
    //         X,
    //         Y,
    //         Width,
    //         Height,
    //         _boxColour);
    //      
    //     Terminal.SetCursorPosition(X, Y);
    //
    //     if (!string.IsNullOrEmpty(Text)) {
    //         Terminal.BackgroundColor = _boxColour;
    //         Terminal.ForegroundColor = ConsoleColor.Black;
    //         Terminal.Write(Text);
    //     }
    //     
    //     Terminal.CursorVisible = true;
    // }
    //
    // protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    // {
    //     Result = InputBoxResult.None;
    //     handled = true;
    //
    //     using TerminalColourRestorer _ = new();
    //
    //     Terminal.BackgroundColor = _boxColour;
    //     Terminal.ForegroundColor = ConsoleColor.Black;
    //     
    //     switch (keyInfo.Key) {
    //         case ConsoleKey.Enter:
    //             Result = InputBoxResult.Enter;
    //             break;
    //         
    //         case ConsoleKey.Escape:
    //             _buffer.Clear();
    //             Result = InputBoxResult.Cancel;
    //             break;
    //         
    //         case ConsoleKey.Backspace:
    //             if (_cursorBufferPosition > 0) {
    //                 _buffer.Remove(_cursorBufferPosition - 1, 1);
    //                 _cursorBufferPosition--;
    //                 Terminal.CursorLeft--;
    //                 Terminal.Write(_buffer.ToString().Substring(_cursorBufferPosition) + " ");
    //                 Terminal.CursorLeft = Terminal.CursorLeft - (_buffer.Length - _cursorBufferPosition + 1);
    //             }
    //             break;
    //         
    //         case ConsoleKey.Delete:
    //             if (_cursorBufferPosition < _buffer.Length) {
    //                 _buffer.Remove(_cursorBufferPosition, 1);
    //                 Terminal.Write(_buffer.ToString().Substring(_cursorBufferPosition) + " ");
    //                 Terminal.CursorLeft = Terminal.CursorLeft - (_buffer.Length - _cursorBufferPosition + 1);
    //             }
    //             break;
    //         
    //         case ConsoleKey.LeftArrow:
    //             if (_cursorBufferPosition > 0) {
    //                 Terminal.CursorLeft--;
    //                 _cursorBufferPosition--;
    //             }
    //             break;
    //         
    //         case ConsoleKey.RightArrow:
    //             if (_cursorBufferPosition < _buffer.Length) {
    //                 Terminal.CursorLeft++;
    //                 _cursorBufferPosition++;
    //             }
    //             break;
    //         
    //         case ConsoleKey.Insert:
    //             _bufferInsertMode = !_bufferInsertMode;
    //             break;
    //         
    //         default:
    //             if (!char.IsControl(keyInfo.KeyChar)) {
    //                 // Buffer insert mode.
    //                 if (_bufferInsertMode) {
    //                     _buffer.Insert(_cursorBufferPosition, keyInfo.KeyChar);
    //                     int currentCursorPosition = Terminal.CursorLeft;
    //                     Terminal.Write(_buffer.ToString().Substring(_cursorBufferPosition));
    //                     _cursorBufferPosition++;
    //                     Terminal.CursorLeft = currentCursorPosition + 1;
    //                 }
    //                 // Buffer overwrite mode.
    //                 else {
    //                     if (_cursorBufferPosition < _buffer.Length) {
    //                         // Overwrite existing char.
    //                         _buffer[_cursorBufferPosition] = keyInfo.KeyChar;
    //                         Terminal.Write(keyInfo.KeyChar);
    //                         _cursorBufferPosition++;
    //                     }
    //                     else {
    //                         // Overwrite at the end behaves like an insert.
    //                         _buffer.Append(keyInfo.KeyChar);
    //                         Terminal.Write(keyInfo.KeyChar);
    //                         _cursorBufferPosition++;
    //                     }
    //                 }
    //             }
    //
    //             break;
    //     }
    // }
    //
    // public InputBoxResult Result { get; private set; } = InputBoxResult.Enter;
    //
    // public void ShowInputBox()
    // {
    //     OnResize();
    //     OnDraw();
    // }
    //
    // public string Text => _buffer.ToString();
    //
    // public string Title { get; set; } = string.Empty;
