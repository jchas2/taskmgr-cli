using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Task.Manager.Interop.Mach;

namespace Task.Manager.System;

public partial class SystemInfo
{
#if __APPLE__
    private const int NanosecondsTo100NanosecondsFactor = 100;
    
    private static TimeSpan CalculateSystemTime(ulong systemTime)
    {
        return new TimeSpan(
            Convert.ToInt64(systemTime / NanosecondsTo100NanosecondsFactor * 1 / 1));
    }
    
    
    private static unsafe bool GetCpuInfoInternal(SystemStatistics systemStatistics)
    {
        systemStatistics.CpuCores = (ulong)Environment.ProcessorCount;
        systemStatistics.CpuFrequency = 0;
        systemStatistics.CpuName = string.Empty;

        ReadOnlySpan<int> sysctlName = [(int)Sys.Selectors.CTL_HW, (int)Sys.Hardware.HW_MODEL];

        byte* buffer = null;
        int bytesLength = 0;

        if (false == Sys.Sysctl(sysctlName, ref buffer, ref bytesLength)) {
            return false;
        }

        /* Byte array returned contains a null terminator byte. */
        var chars = new byte[bytesLength - 1];
        Marshal.Copy((IntPtr)buffer, chars, 0, bytesLength - 1);
        systemStatistics.CpuName = Encoding.UTF8.GetString(chars);

        /* Sysctl does not work for Frequency on Apple M silicon chips, need alternative. */        
        
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

        long cpuTicks = CalculateSystemTime(info.cpu_ticks[MachHost.CPU_STATE_IDLE]).Ticks;
        long kernelTicks = CalculateSystemTime(info.cpu_ticks[MachHost.CPU_STATE_SYSTEM]).Ticks;
        long userTicks = CalculateSystemTime(info.cpu_ticks[MachHost.CPU_STATE_USER]).Ticks;
        
        systemTimes.Idle = (long)cpuTicks;
        systemTimes.Kernel = (long)kernelTicks;
        systemTimes.User = (long)userTicks;

        Marshal.FreeHGlobal(cpuInfoPtr);
        
        return true;
    }

    private static unsafe bool GetSystemMemoryInternal(SystemStatistics systemStatistics)
    {
        systemStatistics.AvailablePageFile = 0;
        systemStatistics.AvailablePhysical = 0;
        systemStatistics.AvailableVirtual = 0;
        systemStatistics.TotalPageFile = 0;
        systemStatistics.TotalPhysical = 0;
        systemStatistics.TotalVirtual = 0;
        
        ReadOnlySpan<int> sysctlName = [(int)Sys.Selectors.CTL_HW, (int)Sys.Hardware.HW_MEMSIZE];

        byte* buffer = null;
        int bytesLength = 0;

        if (false == Sys.Sysctl(sysctlName, ref buffer, ref bytesLength)) {
            return false;
        }

        if (bytesLength == 8) {
            systemStatistics.TotalPhysical = *(ulong*)buffer;
        }
        
        return true;
    }

    private static bool IsRunningAsRootInternal() =>
        UniStd.geteuid() == 0;
#endif
}
