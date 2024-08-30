using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Win32;

// Following declarations are found in the platform sdk header file ProcessThreadsApi.h

public class ProcessThreadsApi
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetSystemTimes(
        out FILETIME idleTime, 
        out FILETIME kernelTime, 
        out FILETIME userTime);
}
