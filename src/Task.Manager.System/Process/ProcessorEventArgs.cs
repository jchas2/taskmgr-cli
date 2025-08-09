namespace Task.Manager.System.Process;

public class ProcessorEventArgs(List<ProcessInfo> processInfos, SystemStatistics systemStatistics)
    : EventArgs
{
    public readonly List<ProcessInfo> ProcessInfos = processInfos;
    public readonly SystemStatistics SystemStatistics = systemStatistics;
}
