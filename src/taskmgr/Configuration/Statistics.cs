namespace Task.Manager.Configuration;

[Flags]
public enum Statistics
{
    Pid     = 1 << 0,
    Process = 1 << 1,
    User    = 1 << 2,
    Pri     = 1 << 3,
    Cpu     = 1 << 4,
    Mem     = 1 << 5,
    Virt    = 1 << 6,
    Thrd    = 1 << 7,
    Disk    = 1 << 8
}
