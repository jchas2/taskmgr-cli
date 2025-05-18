using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Win32;

// Following declarations are found in the platform sdk header file ProfileApi.h

public static class ProfileApi
{
    [DllImport(Libraries.Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern unsafe bool QueryPerformanceFrequency(uint* frequency);
}
