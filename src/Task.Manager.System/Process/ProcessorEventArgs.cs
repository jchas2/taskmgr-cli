namespace Task.Manager.System.Process;

public class ProcessorEventArgs(ProcessInfo[] processInfos, SystemStatistics systemStatistics)
    : EventArgs
{
    public readonly ProcessInfo[] ProcessInfos = processInfos;
    public readonly SystemStatistics SystemStatistics = systemStatistics;
}
