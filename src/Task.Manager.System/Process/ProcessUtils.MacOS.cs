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
    
    internal static string GetProcessCommandLine(in SysDiag::ProcessModule processModule, in string defaultValue)
    {
        // TODO: Determine for Mach. 
        try {
            return processModule.FileName ?? defaultValue;
        }
        catch {
            return defaultValue;
        } 
    }

    public static ulong GetProcessIoOperations(in int pid)
    {
        if (!TryGetProcessByPid(pid, out SysDiag::Process? process)) {
            return 0;
        }
        
        return GetProcessIoOperations(process!);
    }

    public static ulong GetProcessIoOperations(in SysDiag::Process process)
    {
        // TODO:
        return 0;
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

    internal static unsafe string GetProcessUserName(global::System.Diagnostics.Process process)
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
#endif
}
