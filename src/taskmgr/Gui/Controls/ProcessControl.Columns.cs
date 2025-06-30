using System.ComponentModel;

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
    
    internal enum Columns
    {
        [Description("PROCESS")]
        Process = 0,
        [Description("PID")]
        Pid,
        [Description("USER")]
        User,
        [Description("PRI")]
        Priority,
        [Description("CPU%")]
        Cpu,
        [Description("THRDS")]
        Threads,
        [Description("MEM")]
        Memory,
        [Description("PATH")]
        CommandLine,
        Count
    }
}
