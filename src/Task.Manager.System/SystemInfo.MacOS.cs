namespace Task.Manager.System;

public static partial class SystemInfo
{
#if __APPLE__
    public static bool GetCpuTimes(
        out long idle,
        out long kernel,
        out long user)
    {
//        IntPtr host = MachHost.host_self();
//        int count = (int)(Marshal.SizeOf(typeof(MachHost.HostCpuLoadInfo)) / sizeof(int));
        
//        var info = new MachHost.HostCpuLoadInfo() {
//            idle = 0,
//            nice = 0,
//            system = 0,
//            user = 0
//        };
        
//        IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(info));
        
//        if (MachHost.host_statistics(
//            host,
//            MachHost.HOST_CPU_LOAD_INFO,
//            infoPtr,
//            ref count) == 0) {

//            info = Marshal.PtrToStructure<MachHost.HostCpuLoadInfo>(infoPtr);
//            systemTimes.Idle = info.idle;
//            systemTimes.Kernel = info.system;
//            systemTimes.User = info.user;

    }
#endif
}
