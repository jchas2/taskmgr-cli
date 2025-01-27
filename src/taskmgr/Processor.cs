using System.Runtime.ExceptionServices;
using Task.Manager.System;
using Task.Manager.System.Process;

namespace Task.Manager;

public class Processor : IProcessor
{
    private readonly IProcesses _processes;
    private readonly ISystemInfo _systemInfo;

    public Processor(IProcesses processes, ISystemInfo systemInfo)
    {
        _processes = processes ?? throw new ArgumentNullException(nameof(processes));
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
    }

    public ProcessInfo[] GetProcesses()
    {
        int updateTime = 1000;

        GetSystemTimes(out SystemTimes prevSysTimes);
        var allProcs = _processes.GetAll();

        Thread.Sleep(updateTime);
        GetSystemTimes(out SystemTimes sysTimes);

        var sysTimesDeltas = new SystemTimes {
            Idle = sysTimes.Idle - prevSysTimes.Idle,
            Kernel = sysTimes.Kernel - prevSysTimes.Kernel,
            User = sysTimes.User - prevSysTimes.User
        };

        long totalSysTime = sysTimesDeltas.Kernel + sysTimesDeltas.User;

        for (int i = 0; i < allProcs.Length; i++) {
            var currTimes = new ProcessTimeInfo();
            _processes.GetProcessTimes(allProcs[i].Pid, ref currTimes);
            allProcs[i].CurrentTimes = currTimes;
            
            long procKernelDiff = allProcs[i].CurrentTimes.KernelTime - allProcs[i].PreviousTimes.KernelTime;
            long procUserDiff = allProcs[i].CurrentTimes.UserTime - allProcs[i].PreviousTimes.UserTime;
            long totalProc = procKernelDiff + procUserDiff;

            if (totalSysTime == 0) {
                continue;
            }

            allProcs[i].ProcessorTime = (double)((100.0 * (double)totalProc) / (double)totalSysTime);
            allProcs[i].ProcessorKernelTime = (double)((100.0 * (double)procKernelDiff) / (double)totalSysTime);
            allProcs[i].ProcessorUserTime = (double)((100.0 * (double)procUserDiff) / (double)totalSysTime);
        }

        return allProcs;
    }

    private void GetSystemTimes(out SystemTimes systemTimes)
    {
        systemTimes = new SystemTimes();

        if (false == _systemInfo.GetCpuTimes(ref systemTimes)) {
            systemTimes.Idle = 0;
            systemTimes.Kernel = 0;
            systemTimes.User = 0;
        }
    }
}
