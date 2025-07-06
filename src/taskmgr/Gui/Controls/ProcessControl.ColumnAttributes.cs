namespace Task.Manager.Gui.Controls;

public partial class ProcessControl
{
    public class ColumnTitleAttribute(string title) : Attribute
    {
        public string Title { get; } = title;
    }

    public class ColumnPropertyAttribute(string property) : Attribute
    {
        public string Property { get; } = property;
    }
}