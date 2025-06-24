using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public sealed class ThreadInfo
{
    public int ThreadId { get; set; } = 0;
    public string ThreadState { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    
    public static List<ThreadInfo> GetThreads(int pid)
    {
        if (false == ProcessUtils.TryGetProcessByPid(pid, out SysDiag::Process? process) ||
            process == null) {
            return [];
        }

        if (false == GetThreadsInternal(process, out var threadInfos)) {
            return [];
        }
        
        return threadInfos;
    }

    private static bool GetThreadsInternal(SysDiag::Process process, out List<ThreadInfo> threadInfos)
    {
        threadInfos = new List<ThreadInfo>();
        
        foreach (SysDiag::ProcessThread thread in process.Threads) {
            var threadInfo = new ThreadInfo();
            threadInfo.ThreadId = thread.Id;
            threadInfo.ThreadState = $"{thread.ThreadState}";
            
            threadInfo.Reason = thread.ThreadState == SysDiag.ThreadState.Wait
                ? $"{thread.WaitReason}"
                : string.Empty;

            threadInfo.Priority = thread.CurrentPriority;
            
            threadInfos.Add(threadInfo);
        }
        
        return threadInfos.Count > 0;
    }
}