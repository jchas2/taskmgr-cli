using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

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
    
    [DllImport(Libraries.Advapi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool OpenProcessToken(
        SafeProcessHandle processHandle,
        uint desiredAccess,
        out SafeProcessHandle tokenHandle);
}
