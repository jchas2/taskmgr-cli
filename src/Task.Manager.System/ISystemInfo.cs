namespace Task.Manager.System;

public interface ISystemInfo
{
    bool GetCpuTimes(ref SystemTimes systemTimes);
    bool IsRunningAsRoot();
}