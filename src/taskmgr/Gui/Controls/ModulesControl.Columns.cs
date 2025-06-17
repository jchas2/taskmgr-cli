namespace Task.Manager.Gui.Controls;

public partial class ModulesControl
{
    internal const int ColumnModuleNameWidth = 32;
    internal const int ColumnFileNameWidth = 32;

    internal const int ColumnMargin = 1;    
    
    private enum Columns
    {
        ModuleName = 0,
        FileName,
        Count
    }
}