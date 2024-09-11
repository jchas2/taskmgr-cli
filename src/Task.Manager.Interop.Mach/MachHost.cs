using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

// Following declarations are found in the Mach Kernel header file mach_host.h

public class MachHost
{
    // Const to set the host_statistics64 flavor arg.
    public const int HOST_CPU_LOAD_INFO = 3;

    // Consts used to index into HostCpuLoadInfo::cpu_ticks.
    public const int CPU_STATE_USER = 0;
    public const int CPU_STATE_SYSTEM = 1;
    public const int CPU_STATE_IDLE = 2;
    public const int CPU_STATE_NICE = 3;

    [DllImport("libSystem.dylib", SetLastError = true)]
    public static extern IntPtr host_self();

    [StructLayout(LayoutKind.Sequential)]
    public struct HostCpuLoadInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ulong[] cpu_ticks;
    }
    
    [DllImport("libSystem.dylib")]
    public static extern int host_statistics64(
        IntPtr host, 
        int flavor, 
        IntPtr hostInfo, 
        ref int hostInfoCount);    
}
