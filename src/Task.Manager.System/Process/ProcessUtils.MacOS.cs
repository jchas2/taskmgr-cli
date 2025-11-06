using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Task.Manager.Interop.Mach;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public static partial class ProcessUtils
{
#if __APPLE__
    internal static uint GetHandleCountInternal(SysDiag::Process process)
    {
        return (uint)process.HandleCount;
    }

    private static unsafe int GetPriorityInternal(in SysDiag::Process process)
    {
        ProcInfo.proc_taskallinfo? info = GetProcessInfoById(process.Id);

        if (null == info) {
            return 0;
        }
        
        ProcInfo.proc_taskallinfo ti = info.Value;

        return ti.pbsd.pbi_nice;
    }
    
    internal static string GetProcessCommandLine(in int pid, in SysDiag::ProcessModule processModule, in string defaultValue)
    {
        // TODO: Use Pid and determine for Mach. LaunchCtl? 
        try {
            return processModule.FileName ?? defaultValue;
        }
        catch {
            return defaultValue;
        } 
    }
    
    public static ulong GetProcessIoOperations(in int pid) => 
        GetProcessIoOperationsInternal(pid);

    public static ulong GetProcessIoOperations(in SysDiag::Process process) =>
        GetProcessIoOperationsInternal(process.Id);

    private static unsafe ulong GetProcessIoOperationsInternal(in int pid)
    {
        SysResource.rusage_info_v3 info = new();

        int result = LibProc.proc_pid_rusage(pid, SysResource.RUSAGE_INFO_V3, &info);
        if (result < 0) {
            return 0;
        }

        return info.ri_diskio_bytesread + info.ri_diskio_byteswritten;
    }

    private static unsafe ProcInfo.proc_taskallinfo? GetProcessInfoById(int pid)
    {
        int size = sizeof(ProcInfo.proc_taskallinfo);
        ProcInfo.proc_taskallinfo info = default(ProcInfo.proc_taskallinfo);
        
        int result = ProcInfo.proc_pidinfo(
            pid, 
            ProcInfo.PROC_PIDTASKALLINFO, 
            0, 
            &info, 
            size);
        
        return result == size 
            ? new ProcInfo.proc_taskallinfo?(info) 
            : null;
    }
    
    internal static unsafe string GetProcessProductName(
        in SysDiag::ProcessModule processModule,
        in int pid,
        string defaultValue)
    {
        // For now, will simply return the process display name as per proc_bsdinfo.
        ProcInfo.proc_taskallinfo? info = GetProcessInfoById(pid);

        if (null == info) {
            return string.Empty;
        }
        
        ProcInfo.proc_taskallinfo ti = info.Value;
        string? processName = Marshal.PtrToStringUTF8(new IntPtr(ti.pbsd.pbi_name));

        return processName ?? defaultValue;
    }

    internal static unsafe string GetProcessUserName(SysDiag::Process process)
    {
        ProcInfo.proc_taskallinfo? info = GetProcessInfoById(process.Id);

        if (null == info) {
            return string.Empty;
        }

        uint uid = info.Value.pbsd.pbi_uid;
        const int bufferSize = Pwd.Passwd.InitialBufferSize;
        byte* buf = stackalloc byte[bufferSize];

        Pwd.Passwd passwd;
        
        int error = Pwd.GetPwUidR(
            uid, 
            out passwd, 
            buf, 
            bufferSize);
        
        if (0 == error && null != passwd.Name) {
            return Marshal.PtrToStringUTF8((IntPtr)passwd.Name) ?? string.Empty;
        }
        
        return string.Empty;
    }

    internal static bool IsDaemonInternal(in int pid)
    {
        const int LAUNCHD_PID = 1;
        
        ProcInfo.proc_taskallinfo? info = GetProcessInfoById(pid);

        if (null == info) {
            return false;
        }
        
        ProcInfo.proc_taskallinfo ti = info.Value;

        return ti.pbsd.pbi_ppid == LAUNCHD_PID || pid == LAUNCHD_PID;
    }

    internal static unsafe bool IsLowPriorityInternal(in SysDiag::Process process) =>
        GetPriorityInternal(process) > 0;

#endif
}
