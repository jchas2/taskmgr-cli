using System.Diagnostics;
using System.Runtime.InteropServices;
using Task.Manager.Interop.Mach;

namespace Task.Manager.System;

public partial class SystemInfo
{
#if __APPLE__
    private static unsafe bool GetCpuInfoInternal(ref SystemStatistics systemStatistics)
    {
        systemStatistics.CpuCores = (ulong)Environment.ProcessorCount;
        systemStatistics.CpuFrequency = 0;
        systemStatistics.CpuName = string.Empty;

        ReadOnlySpan<int> sysctlName = [6, 24];

        byte* pBuffer = null;
        int bytesLength = 0;

        Sys.Sysctl(sysctlName, ref pBuffer, ref bytesLength);
        
        return true;
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

    private static bool GetSystemMemoryInternal(ref SystemStatistics systemStatistics)
    {
        
        
        return false;        
    }

    private static bool IsRunningAsRootInternal() =>
        UniStd.geteuid() == 0;
#endif
}
