namespace Task.Manager.System;

public interface ISystemInfo
{
    bool GetCpuTimes(ref SystemTimes systemTimes);
    bool GetSystemInfo(ref SystemStatistics systemStatistics);
    bool GetSystemMemory(ref SystemStatistics systemStatistics);
    bool IsRunningAsRoot();
}
