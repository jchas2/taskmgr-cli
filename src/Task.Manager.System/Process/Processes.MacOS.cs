using System.Runtime.InteropServices;
using Task.Manager.Interop.Mach;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public partial class Processes : IProcesses
{
#if __APPLE__    
    private string GetProcessCommandLine(global::System.Diagnostics.Process process)
    {
        // TODO: Determine for Mach
        try {
            return process.MainModule?.FileName ?? process.ProcessName;
        }
        catch {
            return Process.ProcessName;
        }
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
    
    private string GetProcessProductName(SysDiag::Process process)
    {
        return process.ProcessName;
    }

    private unsafe string GetProcessUserName(global::System.Diagnostics.Process process)
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
            string userName = Marshal.PtrToStringUTF8((IntPtr)passwd.Name) ?? string.Empty;
            return userName;
        }
        
        return string.Empty;
    }
#endif
}