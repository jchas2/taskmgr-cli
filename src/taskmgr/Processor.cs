using Task.Manager.System.Process;

namespace Task.Manager;

public class Processor
{
    private readonly IProcesses _processes;

    public Processor(IProcesses processes)
    {
        _processes = processes ?? throw new ArgumentNullException(nameof(processes));
    }
    
    // public IList<ProcessInfo> GetProcesses()
    // {
    //     var allProcs = _processes.GetAll();
    //
    //     return null;
    // }
}