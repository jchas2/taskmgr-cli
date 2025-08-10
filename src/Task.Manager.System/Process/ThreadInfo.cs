using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public sealed class ThreadInfo
{
    public int ThreadId { get; set; } = 0;
    public string ThreadState { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    public long StartAddress { get; set; } = IntPtr.Zero;
    public TimeSpan CpuKernelTime { get; set; } = TimeSpan.Zero;
    public TimeSpan CpuUserTime { get; set; } = TimeSpan.Zero;
    public TimeSpan CpuTotalTime { get; set; } = TimeSpan.Zero;
    
    public static List<ThreadInfo> GetThreads(int pid)
    {
        if (!ProcessUtils.TryGetProcessByPid(pid, out SysDiag::Process? process) ||
            process == null) {
            return [];
        }

        if (!TryGetThreadsInternal(process, out var threadInfos)) {
            return [];
        }
        
        return threadInfos;
    }

    private static bool TryGetThreadsInternal(SysDiag::Process process, out List<ThreadInfo> threadInfos)
    {
        threadInfos = new List<ThreadInfo>();

        try {
            
            foreach (SysDiag::ProcessThread thread in process.Threads) {

                ThreadInfo threadInfo = new() {
                    ThreadId = thread.Id,
                    ThreadState = $"{thread.ThreadState}",
                    Reason = thread.ThreadState == SysDiag.ThreadState.Wait
                        ? $"{thread.WaitReason}"
                        : string.Empty,
                    Priority = thread.CurrentPriority,
                    StartAddress = thread.StartAddress.ToInt64(),
                    CpuKernelTime = thread.PrivilegedProcessorTime,
                    CpuUserTime = thread.UserProcessorTime,
                    CpuTotalTime = thread.TotalProcessorTime
                };

                threadInfos.Add(threadInfo);
            }
            
            return true;
        }
        catch (Exception e) {
            SysDiag.Trace.WriteLine(e);
            return false;
        }
    }
}
