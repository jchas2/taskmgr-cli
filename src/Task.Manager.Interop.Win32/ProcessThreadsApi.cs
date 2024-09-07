using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Win32;

// Following declarations are found in the platform sdk header file ProcessThreadsApi.h

public static class ProcessThreadsApi
{
    [DllImport(Libraries.Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetSystemTimes(
        out MinWinBase.FILETIME idleTime,
        out MinWinBase.FILETIME kernelTime,
        out MinWinBase.FILETIME userTime);
}
