using Task.Manager.System.Process;

namespace Task.Manager.Tests.Process;

public sealed class ProcessServiceFake : IProcessService
{
    private readonly List<ProcessInfo> processInfos = [];

    public void AddProcessInfo(ProcessInfo processInfo) =>
        processInfos.Add(processInfo);

    public ProcessInfo[] GetProcesses() => processInfos.ToArray();

    public ProcessInfo? GetProcessById(int pid) =>
        processInfos.SingleOrDefault(p => p.Pid == pid);
}