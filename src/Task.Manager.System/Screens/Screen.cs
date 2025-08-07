using Task.Manager.System.Controls;
using Task.Manager.System.Controls.MessageBox;

namespace Task.Manager.System.Screens;

public partial class Screen : Control
{
    private readonly MessageBox _messageBox;
    Action? _onMessageBoxResult;

    private const int MessageBoxWidth = 48;
    private const int MessageBoxHeight = 11;

    public Screen(ISystemTerminal systemTerminal) : base(systemTerminal)
    {
        _messageBox = new MessageBox(systemTerminal) {
            Width = MessageBoxWidth,
            Height = MessageBoxHeight,
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
    }

    protected override void OnDraw()
    {
        base.OnDraw();

        if (_messageBox.Visible) {
            _messageBox.Draw();
        }
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        base.OnKeyPressed(keyInfo, ref handled);
        
        if (handled || !_messageBox.Visible) {
            return;
        }

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

    protected override void OnLoad()
    {
        base.OnLoad();
        
        _messageBox.Load();
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
    }

    public void Show()
    {
        Load();
        Clear();
        Resize();
        Draw();
        IsActive = true;
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
