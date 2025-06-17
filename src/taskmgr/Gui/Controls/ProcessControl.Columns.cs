namespace Task.Manager.Gui.Controls;

public partial class ProcessControl
{
    internal const int ColumnProcessWidth = 32;
    internal const int ColumnPidWidth = 7;
    internal const int ColumnUserWidth = 16;
    internal const int ColumnPriorityWidth = 4;
    internal const int ColumnCpuWidth = 7;
    internal const int ColumnThreadsWidth = 7;
    internal const int ColumnMemoryWidth = 10;
    internal const int ColumnCommandlineWidth = 32;

    internal const int ColumnMargin = 1;    
    
    private enum Columns
    {
        Process = 0,
        Pid,
        User,
        Priority,
        Cpu,
        Threads,
        Memory,
        CommandLine,
        Count
    }
}
