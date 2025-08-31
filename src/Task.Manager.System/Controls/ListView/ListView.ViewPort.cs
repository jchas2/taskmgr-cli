using System.Drawing;

namespace Task.Manager.System.Controls.ListView;

public sealed class ViewPort
{
    public int CurrentIndex { get; set; } = 0;
    public int SelectedIndex { get; set; } = 0;
    public int PreviousSelectedIndex { get; set; } = 0;
    public int RowCount { get; set; } = 0;
    public Rectangle Bounds { get; set; } = new Rectangle();

    public void Reset()
    {
        CurrentIndex = 0;
        SelectedIndex = 0;
        PreviousSelectedIndex = 0;
        RowCount = 0;
        Bounds = new Rectangle();
    }
}
