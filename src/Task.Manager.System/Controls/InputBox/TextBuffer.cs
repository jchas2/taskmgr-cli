using System.Text;

namespace Task.Manager.System.Controls.InputBox;

public class TextBuffer
{
    private StringBuilder _buffer = new();
    private int _cursorBufferPosition = 0;
    
    public int CursorBufferPosition => _cursorBufferPosition;
    
    public int Length => _buffer.Length;
    
    public string Text => _buffer.ToString();
    
    public void Clear()
    {
        _buffer.Clear();
        _cursorBufferPosition = 0;
    }

    public bool MoveBackwards()
    {
        if (_cursorBufferPosition == 0) {
            return false;
        }
        
        _buffer.Remove(_cursorBufferPosition - 1, 1);
        _cursorBufferPosition--;
        
        return true;
    }

    public bool Delete()
    {
        if (_cursorBufferPosition == _buffer.Length) {
            return false;
        }

        _buffer.Remove(_cursorBufferPosition, 1);
        
        return true;
    }

    public bool MoveLeft()
    {
        if (_cursorBufferPosition == 0) {
            return false;
        }
        
        _cursorBufferPosition--;

        return true;
    }

    public bool MoveRight()
    {
        if (_cursorBufferPosition == _buffer.Length) {
            return false;
        }

        _cursorBufferPosition++;
        
        return true;
    }

    public bool InsertMode { get; set; } = true;

    public bool Add(char ch)
    {
        if (char.IsControl(ch)) {
            return false;
        }
        
        if (InsertMode) {
            _buffer.Insert(_cursorBufferPosition, ch);
            _cursorBufferPosition++;
        }
        else {
            if (_cursorBufferPosition < _buffer.Length) {
                _buffer[_cursorBufferPosition] = ch;
                _cursorBufferPosition++; 
            }
            else {
                // Overwrite at the end behaves like an insert.
                _buffer.Append(ch);
                _cursorBufferPosition++;
            }
        }

        return true;
    }
}
