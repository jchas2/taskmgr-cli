namespace Task.Manager.Gui.Controls;

public partial class ThreadsControl
{
    internal const int ColumnIdWidth = 12;
    internal const int ColumnStateWidth = 16;
    internal const int ColumnReasonWidth = 24;
    internal const int ColumnPriorityWidth = 4;

    internal const int ColumnMargin = 1;

    public enum Columns
    {
        Id = 0,
        State = 1,
        Reason = 2,
        Priority = 3,
        Count
    }
}
