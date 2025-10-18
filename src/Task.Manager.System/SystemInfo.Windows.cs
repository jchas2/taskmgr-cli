using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Task.Manager.Interop.Win32;

#if __WIN32__
using Microsoft.Win32;
#endif

namespace Task.Manager.System;

#pragma warning disable CA1416 // Validate platform compatibility        

public partial class SystemInfo
{
#if __WIN32__
    private static bool GetCpuInfoInternal(ref SystemStatistics systemStatistics)
    {
        const string RegPath = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0\";
        const string RegKeyProcessorName = "ProcessorNameString";
        const string RegKeyFrequencyMhz = "~Mhz";
        
        systemStatistics.CpuCores = (ulong)Environment.ProcessorCount;
        systemStatistics.CpuFrequency = 0;
        systemStatistics.CpuName = string.Empty;

        RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegPath);
#if DEBUG
        Debug.Assert(null != key, $"Failed OpenSubKey() {RegPath}");
#endif
        if (null == key) {
            return false;
        }
        
        /* REG_SZ */
        object? processorName = key.GetValue(RegKeyProcessorName);
        if (null == processorName) {
            return false;
        }
        
        systemStatistics.CpuName = processorName.ToString() ?? string.Empty;    

        /* Reg DWORD */
        object? frequency = key.GetValue(RegKeyFrequencyMhz);
        if (null == frequency) {
            return false;
        }

        if (!int.TryParse(frequency.ToString() ?? "0", out int frequencyInt32)) {
            return false;
        }
        
        systemStatistics.CpuFrequency = frequencyInt32;
        return true;
    }
    
    private static unsafe bool GetCpuTimesInternal(ref SystemTimes systemTimes)
    {
        MinWinBase.FILETIME lpIdleFileTime;
        MinWinBase.FILETIME lpKernelFileTime;
        MinWinBase.FILETIME lpUserFileTime;

        if (!ProcessThreadsApi.GetSystemTimes( 
            &lpIdleFileTime,
            &lpKernelFileTime,
            &lpUserFileTime)) {
            Win32ErrorHelpers.AssertOnLastError(nameof(ProcessThreadsApi.GetSystemTimes));
            return false;
        }

        systemTimes.Idle = lpIdleFileTime.ToLong();
        // On Windows, Kernel time also includes Idle time.
        systemTimes.Kernel = lpKernelFileTime.ToLong() - systemTimes.Idle;
        systemTimes.User = lpUserFileTime.ToLong();
        
        return true;
	}

    private static unsafe bool GetSystemMemoryInternal(ref SystemStatistics systemStatistics)
    {
        systemStatistics.AvailablePageFile = 0;
        systemStatistics.AvailablePhysical = 0;
        systemStatistics.AvailableVirtual = 0;
        systemStatistics.TotalPageFile = 0;
        systemStatistics.TotalPhysical = 0;
        systemStatistics.TotalVirtual = 0;

        SysInfoApi.MEMORYSTATUSEX memoryStatus = new();
        
        if (!SysInfoApi.GlobalMemoryStatusEx(&memoryStatus)) {
            Win32ErrorHelpers.AssertOnLastError(nameof(SysInfoApi.GlobalMemoryStatusEx));
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
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        return false;
    }
#endif
}

#pragma warning restore CA1416 // Validate platform compatibility
