namespace Task.Manager.System;

public interface ISystemInfo
{
    bool GetCpuTimes(ref SystemTimes systemTimes);
    bool GetSystemStatistics(ref SystemStatistics systemStatistics);
    bool IsRunningAsRoot();
}
