using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Task.Manager.Interop.Win32;

namespace Task.Manager.System;

public partial class SystemInfo
{
#if __WIN32__
    private static bool GetCpuTimesInternal(ref SystemTimes systemTimes)
    {
        MinWinBase.FILETIME idleFileTime;
        MinWinBase.FILETIME kernelFileTime;
        MinWinBase.FILETIME userFileTime;

        if (false == ProcessThreadsApi.GetSystemTimes(
            out idleFileTime,
            out kernelFileTime,
            out userFileTime)) {
#if DEBUG
            int error = Marshal.GetLastWin32Error();
            Debug.Assert(error == 0, $"Failed GetSystemTimes(): {Marshal.GetPInvokeErrorMessage(error)}");
#endif
            return false;
        }

        systemTimes.Idle = idleFileTime.ToLong();
        systemTimes.Kernel = kernelFileTime.ToLong();
        systemTimes.User = userFileTime.ToLong();

        return true;
	}
    
    private static bool IsRunningAsRootInternal()
    {
        /*
         * Even though we have a pre-processor directive for __WIN32__ set in the .csproj
         * the compiler will still complain with a "CA1416: Validate platform compatibility"
         * error unless the .IsOSPlatform guard is in place.
         */
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        return false;
    }
#endif
}
