using System.Drawing;

namespace Task.Manager.System.Controls.ListView;

public class ViewPort
{
    public int CurrentIndex { get; set; } = 0;
    public int SelectedIndex { get; set; } = 0;
    public int PreviousSelectedIndex { get; set; } = 0;
    public int RowCount { get; set; } = 0;
    public int Height { get; set; } = 0;
    public Rectangle Bounds { get; set; } = new Rectangle();
}

