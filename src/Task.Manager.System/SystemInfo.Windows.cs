﻿using System.Diagnostics;
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
    private static bool GetCpuInfoInternal(ref SystemStatistics systemStatistics)
    {
        const string REG_PATH = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0\";
        
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
        var processorName = key.GetValue("ProcessorNameString");
        if (null == processorName) {
            return false;
        }
        
        systemStatistics.CpuName = processorName.ToString() ?? string.Empty;    

        /* Reg DWORD */
        var frequency = key.GetValue("~Mhz");
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

    private static bool GetSystemStatisticsInternal(ref SystemStatistics systemStatistics)
    {
        if (false == ProfileApi.QueryPerformanceFrequency(out long frequency)) {
#if DEBUG
            int error = Marshal.GetLastWin32Error();
            Debug.Assert(error == 0, $"Failed GetSystemTimes(): {Marshal.GetPInvokeErrorMessage(error)}");
#endif
            return false;
        } 
        
        SysInfoApi.GetSystemInfo(out SysInfoApi.SYSTEM_INFO sysInfo); 

        SystemStatistics data = new(); 
        data.CpuFrequency = (double)frequency / 1000000.0; 
        data.CpuCores = (ulong)sysInfo.dwNumberOfProcessors;
        data.CpuName = ""; // TODO.
        data.MachineName = Environment.MachineName;
        
        var os = Environment.OSVersion; 
        data.OsVersion = $"Windows {os.Version.Major}.{os.Version.Minor}.{os.Version.Build}"; 
        
        // TODO: data.PublicIPv4Address = "53.10.87.122";
        // data.PrivateIPv4Address = "192.168.100.12";
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
