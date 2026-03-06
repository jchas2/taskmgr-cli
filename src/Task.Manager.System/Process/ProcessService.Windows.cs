namespace Task.Manager.System.Process;

public sealed partial class ProcessService
{
#if __WIN32__

    private ProcessInfo? GetProcessInfoInternal(int pid)
    {
        return null;        
    }
    
    private IEnumerable<ProcessInfo> GetProcessInfosInternal()
    {
        yield return null;
    }

#endif
}