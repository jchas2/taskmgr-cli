namespace Task.Manager.System.Process;

public partial class Processes : IProcesses
{
#if __APPLE__    
    private string GetProcessUserName(global::System.Diagnostics.Process process)
    {
        return "";
    }
#endif
}