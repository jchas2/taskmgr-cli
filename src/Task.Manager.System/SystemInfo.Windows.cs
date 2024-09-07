using System.Diagnostics;
using System.Runtime.InteropServices;
using Task.Manager.Interop.Win32;

namespace Task.Manager.System;

public static partial class SystemInfo
{
#if __WIN32__
    public static bool GetCpuTimes(ref SystemTimes systemTimes)
	{
        MinWinBase.FILETIME idleFileTime;
        MinWinBase.FILETIME kernelFileTime;
        MinWinBase.FILETIME userFileTime;

        if (false == ProcessThreadsApi.GetSystemTimes(
            out idleFileTime,
            out kernelFileTime,
            out userFileTime)) {

            // TODO: Gracefully write error to debug log.
            int error = Marshal.GetLastWin32Error();
            Debug.WriteLine(error);

            return false;
        }

        systemTimes.Idle = idleFileTime.ToLong();
        systemTimes.Kernel = kernelFileTime.ToLong();
        systemTimes.User = userFileTime.ToLong();

        return true;
	}
#endif
}
