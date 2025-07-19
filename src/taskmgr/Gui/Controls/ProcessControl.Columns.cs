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
    internal const int ColumnHandlesWidth = 8;
    internal const int ColumnMemoryWidth = 10;
    internal const int ColumnCommandlineWidth = 32;

    internal const int ColumnMargin = 1;    
    
    internal enum Columns
    {
        [ColumnTitle("PROCESS")]
        [ColumnProperty("ExeName")]
        Process = 0,
        [ColumnTitle("PID")]
        [ColumnProperty("Pid")]
        Pid,
        [ColumnTitle("USER")]
        [ColumnProperty("UserName")]
        User,
        [ColumnTitle("PRI")]
        [ColumnProperty("BasePriority")]
        Priority,
        [ColumnTitle("CPU%")]
        [ColumnProperty("CpuTimePercent")]
        Cpu,
        [ColumnTitle("THRDS")]
        [ColumnProperty("ThreadCount")]
        Threads,
        [ColumnTitle("HANDLES")]
        [ColumnProperty("HandleCount")]
        Handles,
        [ColumnTitle("MEM")]
        [ColumnProperty("UsedMemory")]
        Memory,
        [ColumnTitle("PATH")]
        [ColumnProperty("CmdLine")]
        CommandLine,
        [ColumnTitle("")]
        [ColumnProperty("")]
        Count
    }
}
