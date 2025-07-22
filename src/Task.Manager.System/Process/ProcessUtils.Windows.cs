using Task.Manager.Interop.Win32;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public static partial class ProcessUtils
{
#if __WIN32__
    public static uint GetHandleCountInternal(SysDiag::Process process)
    {
        IntPtr processHandle = process.Handle;
        uint gdiHandleCount = WinUser.GetGuiResources(processHandle, WinUser.GR_GDIOBJECTS);
        uint usrHandleCount = WinUser.GetGuiResources(processHandle, WinUser.GR_USEROBJECTS);
        
        return (uint)process.HandleCount + (gdiHandleCount + usrHandleCount);
    }
#endif
}