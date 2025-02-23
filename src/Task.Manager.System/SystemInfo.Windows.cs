using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Task.Manager.Interop.Win32;

#if __WIN32__
using Microsoft.Win32;
#endif

namespace Task.Manager.System;

public partial class SystemInfo
{
#if __WIN32__
    private static bool GetCpuInfoInternal(SystemStatistics systemStatistics)
    {
        const string REG_PATH = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0\";
        const string REG_KEY_PROCESSOR_NAME = "ProcessorNameString";
        const string REG_KEY_FREQUENCY_MHZ = "~Mhz";
        
        systemStatistics.CpuCores = (ulong)Environment.ProcessorCount;
        systemStatistics.CpuFrequency = 0;
        systemStatistics.CpuName = string.Empty;

        if (false == RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return false;
        }
        
        /* Compiler will still complain with a "CA1416: Validate platform compatibility" even with the above IsOSPlatform guard */
#pragma warning disable CA1416 // Validate platform compatibility        
        var key = Registry.LocalMachine.OpenSubKey(REG_PATH);
#if DEBUG
        Debug.Assert(null != key, $"Failed OpenSubKey {REG_PATH}");
#endif
        if (null == key) {
            return false;
        }
        
        /* REG_SZ */
        var processorName = key.GetValue(REG_KEY_PROCESSOR_NAME);
        if (null == processorName) {
            return false;
        }
        
        systemStatistics.CpuName = processorName.ToString() ?? string.Empty;    

        /* Reg DWORD */
        var frequency = key.GetValue(REG_KEY_FREQUENCY_MHZ);
        if (null == frequency) {
            return false;
        }

        if (false == Int32.TryParse(frequency.ToString() ?? "0", out int frequencyInt32)) {
            return false;
        }
        
        systemStatistics.CpuFrequency = frequencyInt32;

#pragma warning disable CA1416 // Validate platform compatibility
        return true;
    }
    
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

    private static bool GetSystemMemoryInternal(SystemStatistics systemStatistics)
    {
        systemStatistics.AvailablePageFile = 0;
        systemStatistics.AvailablePhysical = 0;
        systemStatistics.AvailableVirtual = 0;
        systemStatistics.TotalPageFile = 0;
        systemStatistics.TotalPhysical = 0;
        systemStatistics.TotalVirtual = 0;

        SysInfoApi.MEMORYSTATUSEX memoryStatus = new();

        if (false == SysInfoApi.GlobalMemoryStatusEx(ref memoryStatus)) {
#if DEBUG
            int error = Marshal.GetLastWin32Error();
            Debug.Assert(error == 0, $"Failed GlobalMemoryStatusEx(): {Marshal.GetPInvokeErrorMessage(error)}");
#endif
            return false;
        }

        systemStatistics.AvailablePageFile = memoryStatus.ullAvailPageFile;
        systemStatistics.AvailablePhysical = memoryStatus.ullAvailPhys;
        systemStatistics.AvailableVirtual = memoryStatus.ullAvailVirtual;
        systemStatistics.TotalPageFile = memoryStatus.ullTotalPageFile;
        systemStatistics.TotalPhysical = memoryStatus.ullTotalPhys;
        systemStatistics.TotalVirtual = memoryStatus.ullTotalVirtual;

        return true;
    }
    
    private static bool IsRunningAsRootInternal()
    {
        /* Compiler will still complain with a "CA1416: Validate platform compatibility" without guard */
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
