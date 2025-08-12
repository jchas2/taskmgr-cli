using Task.Manager.System.Controls;
using Task.Manager.System.Controls.InputBox;
using Task.Manager.System.Controls.MessageBox;

namespace Task.Manager.System.Screens;

public partial class Screen : Control
{
    private readonly MessageBox messageBox;
    private readonly InputBox inputBox;
    
    Action? onMessageBoxResult;
    Action<string, InputBoxResult>? onInputBoxResult;

    private const int MessageBoxWidth = 48;
    private const int MessageBoxHeight = 11;
    
    private const int InputBoxWidth = 48;

    public Screen(ISystemTerminal systemTerminal) : base(systemTerminal)
    {
        messageBox = new MessageBox(systemTerminal) {
            Width = MessageBoxWidth,
            Height = MessageBoxHeight,
            Visible = false
        };

        inputBox = new InputBox(systemTerminal) {
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
        
        messageBox.Clear();
        inputBox.Clear();
    }

    protected override void OnDraw()
    {
        base.OnDraw();

        if (messageBox.Visible) {
            messageBox.Draw();
        }

        if (inputBox.Visible) {
            inputBox.Draw();
        }
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        base.OnKeyPressed(keyInfo, ref handled);

        if (handled) {
            return;
        }

        if (messageBox.Visible) {
            OnMessageBoxKeyPressed(keyInfo, ref handled);
        }
        else if (inputBox.Visible) {
            OnInputBoxKeyPressed(keyInfo, ref handled);
        }
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        messageBox.Load();
        inputBox.Load();
    }

    private void OnInputBoxKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        inputBox.KeyPressed(keyInfo, ref handled);

        if (inputBox.Result == InputBoxResult.None) {
            return;
        }

        Control.RedrawEnabled = true;
        inputBox.Visible = false;

        onInputBoxResult?.Invoke(inputBox.Text, inputBox.Result);

        Draw();
    }
    
    private void OnMessageBoxKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled)
    {
        messageBox.KeyPressed(keyInfo, ref handled);

        if (messageBox.Result == MessageBoxResult.None) {
            return;
        }

        Control.RedrawEnabled = true;
        messageBox.Visible = false;

        if (messageBox.Result == MessageBoxResult.Ok) {
            onMessageBoxResult?.Invoke();
        }

        Draw();
    }
    
    protected override void OnResize()
    {
        messageBox.X = X + (Width / 2 - messageBox.Width / 2);
        messageBox.Y = Y + (Height / 2 - messageBox.Height / 2);
        messageBox.Resize();
    }
    
    protected override void OnUnload()
    {
        base.OnUnload();
        
        messageBox.Unload();
        inputBox.Unload();
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
        
        onInputBoxResult = onInputResult;

        inputBox.Visible = true;
        inputBox.X = x;
        inputBox.Y = y;
        inputBox.Width = width;
        inputBox.Title = title;
        
        inputBox.ShowInputBox();
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
        
        this.onMessageBoxResult = onMessageBoxResult;

        messageBox.Visible = true;
        messageBox.Text = text;
        messageBox.Title = title;
        
        messageBox.ShowMessageBox();
    }
}
