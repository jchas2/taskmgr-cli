using Task.Manager.System.Process;

namespace Task.Manager;

public interface IProcessor
{
    ProcessInfo[] GetProcesses();
}