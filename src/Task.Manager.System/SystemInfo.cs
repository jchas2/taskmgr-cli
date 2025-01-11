namespace Task.Manager.System;

public partial class SystemInfo : ISystemInfo
{
    public bool GetCpuTimes(ref SystemTimes systemTimes) => GetCpuTimesInternal(ref systemTimes);
    public bool GetSystemStatistics(ref SystemStatistics systemStatistics) => GetSystemStatisticsInternal(ref systemStatistics);
    public bool IsRunningAsRoot() => IsRunningAsRootInternal();

}