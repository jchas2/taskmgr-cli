using Task.Manager.System;
using Task.Manager.System.Process;

namespace Task.Manager.UnitTests;

public sealed class When_Using_Processor
{
    [Fact]
    public void Should_Return_ProcInfos()
    {
        var processes = new Processes();
        var systemInfo = new SystemInfo();
        var processor = new Processor(processes, systemInfo);
        var allProcs = processor.GetProcesses();

        var q = allProcs.OrderByDescending(p => p.ProcessorTime);
        var ql = q.ToList();
    }
}
