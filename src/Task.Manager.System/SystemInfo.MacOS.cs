using System.Runtime.InteropServices;
using Task.Manager.Interop.Mach;

namespace Task.Manager.System;

public partial class SystemInfo
{
#if __APPLE__
    private static bool GetCpuInfoInternal(ref SystemStatistics systemStatistics)
    {
        return false;
    }

    private static bool GetCpuTimesInternal(ref SystemTimes systemTimes)
    {
        IntPtr host = MachHost.host_self();
        int count = (int)(Marshal.SizeOf(typeof(MachHost.HostCpuLoadInfo)) / sizeof(int));
        
        IntPtr cpuInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MachHost.HostCpuLoadInfo)));
        
        if (0 != MachHost.host_statistics64(
                host,
                MachHost.HOST_CPU_LOAD_INFO,
                cpuInfoPtr,
                ref count)) {
            
            return false;
        }

        var info = Marshal.PtrToStructure<MachHost.HostCpuLoadInfo>(cpuInfoPtr);
        
        // TODO: Look at Process.OSX.MapTimes. These ticks could be in nanoseconds, which could
        // be throwing off the calculations later on.
        systemTimes.Idle = (long)info.cpu_ticks[MachHost.CPU_STATE_IDLE];
        systemTimes.Kernel = (long)info.cpu_ticks[MachHost.CPU_STATE_SYSTEM];
        systemTimes.User = (long)info.cpu_ticks[MachHost.CPU_STATE_USER];

        return true;
    }

    private static bool IsRunningAsRootInternal() =>
        UniStd.geteuid() == 0;
#endif
}
