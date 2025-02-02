namespace Task.Manager.System;

public interface ISystemInfo
{
    bool GetCpuTimes(ref SystemTimes systemTimes);
    bool GetSystemInfo(SystemStatistics systemStatistics);
    bool GetSystemMemory(SystemStatistics systemStatistics);
    bool IsRunningAsRoot();
}
