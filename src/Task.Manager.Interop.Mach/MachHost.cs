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
    
    const int HOST_CPU_LOAD_INFO = 3;
    
    [DllImport("libc")]
    static extern int host_statistics(
        int host_priv, 
        int flavor, 
        IntPtr info, 
        ref int count);
}
