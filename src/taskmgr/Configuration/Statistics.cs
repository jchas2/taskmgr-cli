using System.ComponentModel;

namespace Task.Manager.Configuration;

[Flags]
public enum Statistics
{
    Process = 1 << 0,
    Pid     = 1 << 1,
    User    = 1 << 2,
    Pri     = 1 << 3,
    Cpu     = 1 << 4,
    Thrd    = 1 << 5,
    Mem     = 1 << 6,
    Path    = 1 << 7,
    Disk    = 1 << 8
}
