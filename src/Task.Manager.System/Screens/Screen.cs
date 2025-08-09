using Task.Manager.System.Controls;
using Task.Manager.System.Controls.InputBox;
using Task.Manager.System.Controls.MessageBox;

namespace Task.Manager.System.Screens;

public partial class Screen : Control
{
    private readonly MessageBox _messageBox;
    private readonly InputBox _inputBox;
    
    Action? _onMessageBoxResult;
    Action<string, InputBoxResult>? _onInputBoxResult;

    private const int MessageBoxWidth = 48;
    private const int MessageBoxHeight = 11;
    
    private const int InputBoxWidth = 48;

    public Screen(ISystemTerminal systemTerminal) : base(systemTerminal)
    {
        _messageBox = new MessageBox(systemTerminal) {
            Width = MessageBoxWidth,
            Height = MessageBoxHeight,
            Visible = false
        };

        _inputBox = new InputBox(systemTerminal) {
            Width = MessageBoxWidth,
            Height = 1,
            Visible = false
        };
    } 

    public void Close()
    {
        Unload();
        IsActive = false;
    }

    public bool CursorVisible { get; set; } = true;
    
    private bool IsActive { get; set; } = false;

    protected override void OnClear()
    {
        base.OnClear();
        
        _messageBox.Clear();
        _inputBox.Clear();
    }

    protected override void OnDraw()
    {
        base.OnDraw();

        if (_messageBox.Visible) {
            _messageBox.Draw();
        }

        if (_inputBox.Visible) {
            _inputBox.Draw();
        }
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        base.OnKeyPressed(keyInfo, ref handled);

        if (handled) {
            return;
        }

        if (_messageBox.Visible) {
            OnMessageBoxKeyPressed(keyInfo, ref handled);
        }
        else if (_inputBox.Visible) {
            OnInputBoxKeyPressed(keyInfo, ref handled);
        }
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        _messageBox.Load();
        _inputBox.Load();
    }

    private void OnInputBoxKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        _inputBox.KeyPressed(keyInfo, ref handled);

        if (_inputBox.Result == InputBoxResult.None) {
            return;
        }

        Control.RedrawEnabled = true;
        _inputBox.Visible = false;

        _onInputBoxResult?.Invoke(_inputBox.Text, _inputBox.Result);

        Draw();
    }
    
    private void OnMessageBoxKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        _messageBox.KeyPressed(keyInfo, ref handled);

        if (_messageBox.Result == MessageBoxResult.None) {
            return;
        }

        Control.RedrawEnabled = true;
        _messageBox.Visible = false;

        if (_messageBox.Result == MessageBoxResult.Ok) {
            _onMessageBoxResult?.Invoke();
        }

        Draw();
    }
    
    protected override void OnResize()
    {
        _messageBox.X = X + (Width / 2 - _messageBox.Width / 2);
        _messageBox.Y = Y + (Height / 2 - _messageBox.Height / 2);
        _messageBox.Resize();
    }
    
    protected override void OnUnload()
    {
        base.OnUnload();
        
        _messageBox.Unload();
        _inputBox.Unload();
    }

    public void Show()
    {
        Load();
        Clear();
        Resize();
        Draw();
        IsActive = true;
    }

    public void ShowInputBox(
        int x, 
        int y,
        int width,
        string title, 
        Action<string, InputBoxResult> onInputResult)
    {
        ArgumentNullException.ThrowIfNull(title);

        Control.RedrawEnabled = false;
        
        _onInputBoxResult = onInputResult;

        _inputBox.Visible = true;
        _inputBox.X = x;
        _inputBox.Y = y;
        _inputBox.Width = width;
        _inputBox.Title = title;
        
        _inputBox.ShowInputBox();
    }

    public void ShowMessageBox(
        string title,
        string text,
        MessageBoxButtons buttons,
        Action onMessageBoxResult)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(text);

        Control.RedrawEnabled = false;
        
        _onMessageBoxResult = onMessageBoxResult;

        _messageBox.Visible = true;
        _messageBox.Text = text;
        _messageBox.Title = title;
        
        _messageBox.ShowMessageBox();
    }
}
