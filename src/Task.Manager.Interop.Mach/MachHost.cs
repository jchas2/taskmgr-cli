using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

// Following declarations are found in the Mach Kernel header file mach_host.h

public class MachHost
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HostCpuLoadInfo
    {
        public uint user;
        public uint system;
        public uint idle;
        public uint nice;
    }
    
    public const int HOST_CPU_LOAD_INFO = 3;
    
    [DllImport("libSystem.dylib", SetLastError = true)]
    public static extern IntPtr host_self();
    
    [DllImport("libSystem.dylib", SetLastError = true)]
    public static extern int host_statistics(
        IntPtr host_priv, 
        int flavor, 
        IntPtr host_info_out, 
        ref int host_info_outCnt);
}
