using Task.Manager.Interop.Mach;

namespace Task.Manager.System.Process;

public partial class Processes : IProcesses
{
#if __APPLE__    
    
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
    
    private string GetProcessUserName(global::System.Diagnostics.Process process)
    {
        ProcInfo.proc_taskallinfo? info = GetProcessInfoById(process.Id);

        return "";
    }
#endif
}