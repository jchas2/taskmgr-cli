namespace Task.Manager.System.Controls.ListView;

public class ListViewSubItem
{
    private ListViewItem owner;
    private string text;
    private SubItemStyle? style;

    public ListViewSubItem(ListViewItem owner, string text)
    {
        this.owner = owner;
        this.text = text;
    }

    public ListViewSubItem(
        ListViewItem owner,
        string text,
        ConsoleColor backgroundColor,
        ConsoleColor foregroundColor)
    {
        this.owner = owner;
        this.text = text;
        style = new SubItemStyle {
            BackgroundColour = backgroundColor,
            ForegroundColour = foregroundColor
        };
    }

    public ConsoleColor BackgroundColor
    {
        get {
            if (style != null) {
                return style.BackgroundColour;
            }

            return owner.BackgroundColour;
        }
        set {
            style ??= new SubItemStyle();
            
            if (style.BackgroundColour != value) {
                style.BackgroundColour = value;
            }
        }
    }

    internal ListViewItem Owner
    {
        get => owner;
        set => owner = value;
    }
    
    public ConsoleColor ForegroundColor
    {
        get {
            if (style != null) {
                return style.ForegroundColour;
            }

            return owner.ForegroundColour;
        }
        set {
            style ??= new SubItemStyle();
            
            if (style.ForegroundColour != value) {
                style.ForegroundColour = value;
            }
        }
    }

    public string Text
    {
        get => text ?? string.Empty;
        set => text = value;
    }
}