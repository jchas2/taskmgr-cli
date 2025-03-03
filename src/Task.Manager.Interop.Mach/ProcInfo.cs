using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

// Following declarations are found in the Mach Kernel header file proc_info.h

public class ProcInfo
{
    private const int MAXCOMLEN = 16;
    public const int PROC_PIDTASKALLINFO = 2;

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct proc_bsdinfo
    {
        internal uint       pbi_flags;
        internal uint       pbi_status;
        internal uint       pbi_xstatus;
        internal uint       pbi_pid;
        internal uint       pbi_ppid;
        internal uint       pbi_uid;
        internal uint       pbi_gid;
        internal uint       pbi_ruid;
        internal uint       pbi_rgid;
        internal uint       pbi_svuid;
        internal uint       pbi_svgid;
        internal uint       reserved;
        internal fixed byte pbi_comm[MAXCOMLEN];
        internal fixed byte pbi_name[MAXCOMLEN * 2];
        internal uint       pbi_nfiles;
        internal uint       pbi_pgid;
        internal uint       pbi_pjobc;
        internal uint       e_tdev;
        internal uint       e_tpgid;
        internal int        pbi_nice;
        internal ulong      pbi_start_tvsec;
        internal ulong      pbi_start_tvusec;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct proc_taskinfo
    {
        internal ulong   pti_virtual_size;
        internal ulong   pti_resident_size;
        internal ulong   pti_total_user;
        internal ulong   pti_total_system;
        internal ulong   pti_threads_user;
        internal ulong   pti_threads_system;
        internal int     pti_policy;
        internal int     pti_faults;
        internal int     pti_pageins;
        internal int     pti_cow_faults;
        internal int     pti_messages_sent;
        internal int     pti_messages_received;
        internal int     pti_syscalls_mach;
        internal int     pti_syscalls_unix;
        internal int     pti_csw;
        internal int     pti_threadnum;
        internal int     pti_numrunning;
        internal int     pti_priority;
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct proc_taskallinfo
    {
        internal proc_bsdinfo    pbsd;
        internal proc_taskinfo   ptinfo;
    }

    [DllImport(Libraries.LibProc, SetLastError = true)]
    public static extern unsafe int proc_pidinfo(
        int pid,
        int flavor,
        ulong arg,
        proc_taskallinfo* buffer,
        int bufferSize);
}