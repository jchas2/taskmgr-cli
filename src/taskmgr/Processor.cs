using Task.Manager.System;
using Task.Manager.System.Process;

namespace Task.Manager;

public class Processor
{
    private readonly IProcesses _processes;

    public Processor(IProcesses processes)
    {
        _processes = processes ?? throw new ArgumentNullException(nameof(processes));
    }
    
    public IList<ProcessInfo> GetProcesses()
    {
        int updateTime = 1000;
        GetSystemTimes(out SystemTimes prevSystemTimes);
        var allProcs = _processes.GetAll();

        Thread.Sleep(updateTime);

        GetSystemTimes(out SystemTimes systemTimes);

        foreach (var proc in allProcs) {
            var nextTimes = new ProcessTimeInfo();
            _processes.GetProcessTimes(proc.Pid, ref nextTimes);

        }

        return allProcs;
    }

    private void GetSystemTimes(out SystemTimes systemTimes)
    {
        systemTimes = new SystemTimes();
        if (false == SystemInfo.GetCpuTimes(ref systemTimes)) {
            systemTimes.Idle = 0;
            systemTimes.Kernel = 0;
            systemTimes.User = 0;
        }
    }
}
