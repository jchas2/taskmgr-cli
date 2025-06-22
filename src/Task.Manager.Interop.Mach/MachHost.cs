using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

// Following declarations are found in the Mach Kernel header file mach_host.h

public sealed class MachHost
{
    // Const to set the host_statistics64 flavor arg.
    public const int HOST_CPU_LOAD_INFO = 3;

    // Consts used to index into HostCpuLoadInfo::cpu_ticks.
    public const int CPU_STATE_USER = 0;
    public const int CPU_STATE_SYSTEM = 1;
    public const int CPU_STATE_NICE = 2;
    public const int CPU_STATE_IDLE = 3;
    public const int CPU_STATE_MAX = 4;
    
    // Const to set the host_statistics64 flavor arg.
    public const int HOST_VM_INFO64 = 4;
    public const int HOST_VM_INFO64_COUNT = 38;

    [DllImport(Libraries.LibSystemDyLib, SetLastError = true)]
    public static extern IntPtr host_self();

    [StructLayout(LayoutKind.Sequential)]
    public struct HostCpuLoadInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ulong[] cpu_ticks;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VmStatistics64
    {
        public uint  free_count;
        public uint  active_count;
        public uint  inactive_count;
        public uint  wire_count;
        public ulong zero_fill_count;
        public ulong reactivations;
        public ulong pageins;
        public ulong pageouts;
        public ulong faults;
        public ulong cow_faults;
        public ulong lookups;
        public ulong hits;
        public ulong purges;
        public uint  purgeable_count;
        public uint  speculative_count;
        public ulong decompressions;
        public ulong compressions;
        public ulong swapins;
        public ulong swapouts;
        public uint	 compressor_page_count;	
        public uint	 throttled_count;	
        public uint	 external_page_count;	
        public uint	 internal_page_count;	
        public ulong total_uncompressed_pages_in_compressor; 
    }

    [DllImport(Libraries.LibSystemDyLib)]
    public static extern int host_statistics64(
        IntPtr host, 
        int flavor, 
        IntPtr hostInfo, 
        ref int hostInfoCount);
}
