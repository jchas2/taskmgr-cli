using System.Runtime.InteropServices;
using Task.Manager.Interop.Mach;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public static partial class ProcessUtils
{
#if __APPLE__
    public static uint GetHandleCountInternal(SysDiag::Process process)
    {
        return (uint)process.HandleCount;
    }
    
    public static string GetProcessCommandLine(global::System.Diagnostics.Process process)
    {
        // TODO: Determine for Mach
        try {
            return process.MainModule?.FileName ?? process.ProcessName;
        }
        catch {
            return process.ProcessName;
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
        
        return (result == size ? new ProcInfo.proc_taskallinfo?(info) : null);
    }
    
    public static unsafe string GetProcessProductName(SysDiag::Process process)
    {
        // ReadOnlySpan<int> sysctlName = [
        //     (int)Sys.Selectors.CTL_KERN, 
        //     Sys.KERN_PROC, 
        //     Sys.KERN_PROC_PATHNAME, 
        //     process.Id];
        //
        // byte* buffer = null;
        // int bytesLength = 0;
        //
        // if (Sys.Sysctl(sysctlName, ref buffer, ref bytesLength)) {
        //     /* Byte array returned contains a null terminator byte. */
        //     return Encoding.UTF8.GetString(buffer, bytesLength - 1);
        //     // TODO: NativeMemory.Free(pBuffer);
        // }

        // Try:
        // struct kinfo_proc proc;
        // size_t proc_size = sizeof(proc);
        // int mib[4] = { CTL_KERN, KERN_PROC, KERN_PROC_PID, pid };
        //
        // if (sysctl(mib, 4, &proc, &proc_size, NULL, 0) == 0) {
        //     printf("Application Name: %s\n", proc.kp_proc.p_comm);
        // } else {
        //     perror("sysctl");
        // }
        
        return process.ProcessName;
    }

    public static unsafe string GetProcessUserName(global::System.Diagnostics.Process process)
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
