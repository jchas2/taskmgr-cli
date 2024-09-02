using System.Diagnostics;
using System.Runtime.InteropServices;
using Task.Manager.Interop.Mach;
using Task.Manager.Interop.Win32;
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
        var prevSystemTimes = GetSystemTimes();
        var allProcs = _processes.GetAll();

        Thread.Sleep(updateTime);

        var systemTimes = GetSystemTimes();

        foreach (var proc in allProcs) {
            var nextTimes = new ProcessTimeInfo();
            _processes.GetProcessTimes(proc.Pid, ref nextTimes);

        }

        return allProcs;
    }

    private SystemTimes GetSystemTimes()
    {
        SystemTimes systemTimes = new SystemTimes();

#if __WIN32__
        MinWinBase.FILETIME idleTime;
        MinWinBase.FILETIME kernelTime;
        MinWinBase.FILETIME userTime;

        if (ProcessThreadsApi.GetSystemTimes(
            out idleTime,
            out kernelTime,
            out userTime)) {

            systemTimes.Idle = idleTime.ToDateTime();
            systemTimes.Kernel = kernelTime.ToDateTime();
            systemTimes.User = userTime.ToDateTime();
#endif
#if __APPLE__
        IntPtr host = MachHost.host_self();
        int count = (int)(Marshal.SizeOf(typeof(MachHost.HostCpuLoadInfo)) / sizeof(int));
        
        var info = new MachHost.HostCpuLoadInfo() {
            idle = 0,
            nice = 0,
            system = 0,
            user = 0
        };
        
        IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(info));
        
        if (MachHost.host_statistics(
            host,
            MachHost.HOST_CPU_LOAD_INFO,
            infoPtr,
            ref count) == 0) {

            info = Marshal.PtrToStructure<MachHost.HostCpuLoadInfo>(infoPtr);
            systemTimes.Idle = info.idle;
            systemTimes.Kernel = info.system;
            systemTimes.User = info.user;
#endif
        }
        else {
            int error = Marshal.GetLastWin32Error();
            Debug.WriteLine(error);
        }

        return systemTimes;
    }

//#if __WIN32__
//        private void GetSystemTimes2()
//    {
//        MinWinBase.FILETIME idleTime;
//        MinWinBase.FILETIME kernelTime;
//        MinWinBase.FILETIME userTime;

//        if (ProcessThreadsApi.GetSystemTimes(
//            out idleTime,
//            out kernelTime,
//            out userTime)) {

//        }
//        else {
//        }
//    }
//#endif

//#if __APPLE__
//    private MachHost.HostCpuLoadInfo GetHostCpuLoadInfo()
//    {
//        IntPtr host = MachHost.host_self();
//        int count = (int)(Marshal.SizeOf(typeof(MachHost.HostCpuLoadInfo)) / sizeof(int));
        
//        var info = new MachHost.HostCpuLoadInfo() {
//            idle = 0,
//            nice = 0,
//            system = 0,
//            user = 0
//        };
        
//        IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(info));
        
//        if (MachHost.host_statistics(
//                host,
//                MachHost.HOST_CPU_LOAD_INFO,
//                infoPtr,
//                ref count) == 0) {

//            info = Marshal.PtrToStructure<MachHost.HostCpuLoadInfo>(infoPtr);
//            return info;
//        }
//        else {
//            int error = Marshal.GetLastWin32Error();
//            Debug.Write(error);
//        }

//        return info;
//    }
//#endif
}
