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
    
    private static unsafe TimeSpan CalculateSystemTime(ulong systemTime)
    {
        MachTime.mach_timebase_info_data_t timeBase = default;
        int result = MachTime.mach_timebase_info(&timeBase);
        
        Debug.Assert(result == 0, $"Failed mach_timebase_info(): {result}");
        
        if (result != 0) {
            timeBase.denom = 1;
            timeBase.numer = 1;
        }
        
        return new TimeSpan(
            (Convert.ToInt64(systemTime / NanosecondsTo100NanosecondsFactor * timeBase.numer / timeBase.denom)));
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
            
            Marshal.FreeHGlobal(cpuInfoPtr);
            return false;
        }

        var info = Marshal.PtrToStructure<MachHost.HostCpuLoadInfo>(cpuInfoPtr);

        long idleTicks = CalculateSystemTime(info.cpu_ticks[MachHost.CPU_STATE_IDLE]).Ticks;
        long kernelTicks = CalculateSystemTime(info.cpu_ticks[MachHost.CPU_STATE_SYSTEM]).Ticks;
        long userTicks = CalculateSystemTime(info.cpu_ticks[MachHost.CPU_STATE_USER]).Ticks;
        long niceTicks = CalculateSystemTime(info.cpu_ticks[MachHost.CPU_STATE_NICE]).Ticks;
        
        systemTimes.Idle = (long)idleTicks;
        systemTimes.Kernel = (long)kernelTicks;
        systemTimes.User = (long)userTicks + (long)niceTicks;

        Debug.WriteLine($"idleTicks = {idleTicks}, kernelTicks = {kernelTicks}, userTicks = {userTicks}, niceTicks = {niceTicks}");
        
        Marshal.FreeHGlobal(cpuInfoPtr);
        return true;
    }

    private static unsafe int GetPageSize()
    {
        int pageSize = 0;
        ReadOnlySpan<int> sysctlName = [(int)Sys.Selectors.CTL_HW, (int)Sys.Hardware.HW_PAGESIZE];

        byte* buffer = null;
        int bytesLength = 0;

        if (false == Sys.Sysctl(sysctlName, ref buffer, ref bytesLength)) {
            return 0;
        }

        if (bytesLength == sizeof(int)) {
            pageSize = *(int*)buffer;
        }

        return pageSize;
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
        
        IntPtr host = MachHost.host_self();
        int count = (int)(Marshal.SizeOf(typeof(MachHost.VmStatistics64)) / sizeof(int));
        
        IntPtr vmStatisticsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MachHost.VmStatistics64)));
        
        if (0 != MachHost.host_statistics64(
            host,
            MachHost.HOST_VM_INFO64,
            vmStatisticsPtr,
            ref count)) {
            
            Marshal.FreeHGlobal(vmStatisticsPtr);
            return false;
        }

        var info = Marshal.PtrToStructure<MachHost.VmStatistics64>(vmStatisticsPtr);
        int pageSize = GetPageSize();
        systemStatistics.AvailableVirtual = info.free_count * (uint)pageSize;        
        
        Marshal.FreeHGlobal(vmStatisticsPtr);
        
        return true;
    }

    private static bool IsRunningAsRootInternal() =>
        UniStd.geteuid() == 0;
#endif
}
