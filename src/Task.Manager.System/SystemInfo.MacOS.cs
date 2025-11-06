using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Task.Manager.Interop.Mach;

namespace Task.Manager.System;

public static partial class SystemInfo
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
    
    private static bool GetCpuHighCoreUsageInternal(double processUsagePercent) =>
        processUsagePercent >= 100.0;

    private static unsafe bool GetCpuInfoInternal(ref SystemStatistics systemStatistics)
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
        uint numCpus = 0;
        uint numCpuInfo = 0;
        IntPtr cpuInfo = IntPtr.Zero;

        if (0 != MachHost.host_processor_info(
            MachHost.mach_host_self(),
            MachHost.PROCESSOR_CPU_LOAD_INFO,
            out numCpus,
            out cpuInfo,
            out numCpuInfo)) {
            
            return false;
        }

        long idleTicks = 0;
        long kernelTicks = 0;
        long userTicks = 0;
        
        int cpuTicksPerCpu = MachHost.CPU_STATE_MAX;
        uint[] ticks = new uint[cpuTicksPerCpu];

        for (int i = 0; i < numCpus; i++)
        {
            for (int j = 0; j < cpuTicksPerCpu; j++)
            {
                IntPtr tickPtr = IntPtr.Add(cpuInfo, (i * cpuTicksPerCpu + j) * sizeof(uint));
                ticks[j] = (uint)Marshal.ReadInt32(tickPtr);
            }

            userTicks += ticks[MachHost.CPU_STATE_USER] + ticks[MachHost.CPU_STATE_NICE];
            kernelTicks += ticks[MachHost.CPU_STATE_SYSTEM];
            idleTicks += ticks[MachHost.CPU_STATE_IDLE];
        }

        systemTimes.Idle = idleTicks;
        systemTimes.Kernel = kernelTicks;
        systemTimes.User = userTicks;

        IntPtr size = new IntPtr((int)numCpuInfo * sizeof(int));
        MachHost.vm_deallocate(MachHost.mach_task_self(), cpuInfo, size);

        return true;
    }
    
    private static unsafe int GetPageSize()
    {
        int pageSize = 0;
        ReadOnlySpan<int> sysctlName = [(int)Sys.Selectors.CTL_HW, (int)Sys.Hardware.HW_PAGESIZE];

        byte* buffer = null;
        int bytesLength = 0;

        if (!Sys.Sysctl(sysctlName, ref buffer, ref bytesLength)) {
            return 0;
        }

        if (bytesLength == sizeof(int)) {
            pageSize = *(int*)buffer;
        }

        return pageSize;
    }
    
    private static unsafe bool GetSystemMemoryInternal(ref SystemStatistics systemStatistics)
    {
        systemStatistics.AvailablePageFile = 0;
        systemStatistics.AvailablePhysical = 0;
        systemStatistics.AvailableVirtual = 0;
        systemStatistics.TotalPageFile = 0;
        systemStatistics.TotalPhysical = 0;
        systemStatistics.TotalVirtual = 0;
        
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

        // TODO: byte* buffer needs to be freed, or does sysctl stack alloc.
        ReadOnlySpan<int> sysctlName = [(int)Sys.Selectors.CTL_HW, (int)Sys.Hardware.HW_MEMSIZE];
        byte* buffer = null;
        int bytesLength = 0;

        if (!Sys.Sysctl(sysctlName, ref buffer, ref bytesLength) || bytesLength != 8) {
            return false;
        }
        
        ulong totalPhysical = *(ulong*)buffer;
        long totalUsedCount = info.wire_count + info.inactive_count + info.active_count + info.compressor_page_count;
        long totalUsed = totalUsedCount * pageSize;

        systemStatistics.TotalPhysical = totalPhysical;
        systemStatistics.AvailablePhysical = totalPhysical - (ulong)totalUsed;

        // TODO: byte* buffer needs to be freed, or does sysctl stack alloc.
        sysctlName = [(int)Sys.Selectors.CTL_VM, Sys.VM_SWAPUSAGE];
        buffer = null;
        bytesLength = 0;

        if (!Sys.Sysctl(sysctlName, ref buffer, ref bytesLength) || bytesLength != 32) {
            return false;
        }
        
        Sys.XswUsage* xswUsage = (Sys.XswUsage*)buffer;

        systemStatistics.TotalPageFile = xswUsage->total;
        systemStatistics.AvailablePageFile = xswUsage->avail;
        
        Marshal.FreeHGlobal(vmStatisticsPtr);
        
        return true;
    }

    private static bool IsRunningAsRootInternal() =>
        UniStd.geteuid() == 0;
#endif
}
