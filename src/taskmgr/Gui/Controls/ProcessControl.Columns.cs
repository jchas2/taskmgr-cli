namespace Task.Manager.Gui.Controls;

public partial class ProcessControl
{
    internal const int COLUMN_PROCESS_WIDTH = 32;
    internal const int COLUMN_PID_WIDTH = 7;
    internal const int COLUMN_USER_WIDTH = 16;
    internal const int COLUMN_PRIORITY_WIDTH = 4;
    internal const int COLUMN_CPU_WIDTH = 7;
    internal const int COLUMN_THREADS_WIDTH = 7;
    internal const int COLUMN_MEMORY_WIDTH = 10;
    internal const int COLUMN_COMMANDLINE_WIDTH = 32;

    internal const int COLUMN_MARGIN = 1;    
    
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
