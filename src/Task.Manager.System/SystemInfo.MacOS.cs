using System.Runtime.InteropServices;
using Task.Manager.Interop.Mach;

namespace Task.Manager.System;

public static partial class SystemInfo
{
#if __APPLE__
    public static bool GetCpuTimes(ref SystemTimes systemTimes)
    {
        IntPtr host = MachHost.host_self();
        int count = (int)(Marshal.SizeOf(typeof(MachHost.HostCpuLoadInfo)) / sizeof(int));

        var info = new MachHost.HostCpuLoadInfo() {
            idle = 0,
            nice = 0,
            system = 0,
            user = 0
        };

        IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(info));

        if (0 != MachHost.host_statistics(
            host,
            MachHost.HOST_CPU_LOAD_INFO,
            infoPtr,
            ref count)) {
            
            return false;
        }

        info = Marshal.PtrToStructure<MachHost.HostCpuLoadInfo>(infoPtr);
        systemTimes.Idle = info.idle;
        systemTimes.Kernel = info.system;
        systemTimes.User = info.user;

        return true;
    }
}
#endif
