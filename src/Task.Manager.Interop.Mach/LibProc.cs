using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

// Following declarations are found in the Mach Kernel header file mach_host.h

public sealed class LibProc
{
    [DllImport(Libraries.LibProc, SetLastError = true)]
    public static extern unsafe int proc_pid_rusage(
        int pid,
        int flavor,
        SysResource.rusage_info_v3* buffer);
}