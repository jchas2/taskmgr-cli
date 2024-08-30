namespace Task.Manager.System.Process;

public struct ProcessTimeInfo
{
    public TimeSpan KernelTime;
    public TimeSpan UserTime;
    public long DiskOperations;
}
