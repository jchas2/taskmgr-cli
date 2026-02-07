using Moq;
using Task.Manager.Gui.Controls;
using Task.Manager.System;
using Task.Manager.Tests.Common;
using Task.Manager.Tests.Process;
using Xunit.Abstractions;

namespace Task.Manager.Tests.Gui.Controls;

public sealed class HeaderControlTests
{
    private readonly ITestOutputHelper outputHelper;
    private readonly RunContextHelper runContextHelper;
    private readonly RunContext runContext;

    public HeaderControlTests(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;
        runContextHelper = new RunContextHelper();
        runContext = runContextHelper.GetRunContext();
    }

    [Fact]
    public void Constructor_With_Valid_Args_Initialises_Successfully()
    {
        HeaderControl ctrl = new(
            runContext.Processor,
            runContext.Terminal, 
            runContext.AppConfig);

        Assert.NotNull(ctrl);
    }
    
    [Fact]
    public void Constructor_With_Null_Terminal_Throws_ArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() => 
            new HeaderControl(
                runContext.Processor,
                null!,
                runContext.AppConfig));

    [Fact]
    public void Should_Draw_Header()
    {
        ProcessorFake processorFake = new();

        SystemStatistics statistics = new() {
            AvailablePhysical = 935247872,
            AvailablePageFile = 815726592,
            AvailableVirtual = 0,
            TotalPhysical = 38654705664,
            TotalPageFile = 2147483648,
            TotalVirtual = 0,
            CpuFrequency = 0,
            CpuCores = 12,
            CpuName = "Mac15,7",
            CpuPercentIdleTime = 0.7482,
            CpuPercentKernelTime = 0.1188,
            CpuPercentUserTime = 0.1333,
            MachineName = "mach01",
            OsVersion = "Unix 14.1.0",
            PublicIPv4Address = "",
            PrivateIPv4Address = "192.168.1.110",
            DiskUsage = 30037,
            ProcessCount = 739,
            ThreadCount = 4993,
        };

        processorFake.AddSystemStats(statistics);
        
        HeaderControl ctrl = new(
            processorFake,
            runContext.Terminal, 
            runContext.AppConfig) {
            Width = 128,
            Height = 9
        };

        ctrl.Load();
        ctrl.Resize();
        processorFake.RaiseProcessorUpdatedEvent();
        
        Assert.True(runContext.AppConfig.MetreStyle == MetreControlStyle.Dots);
        
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("TASK MANAGER"))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("mach01  (Unix 14.1.0)  IP 192.168.1.110"))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Mac15,7 (Cores 12)"))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Cpu"))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("⣿⣿⣿⣿⣿"))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("⣿⣿⣿⣿⣿⣿"))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("025.2%"))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Mem"))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿"))), Times.AtLeastOnce);
        //runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("35.1 GB/36 GB"))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Swp") || s.Contains("Vir"))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿"))), Times.AtLeastOnce);
        //runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("1.2 GB/2 GB"))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Dsk"))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿"))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("0.1 MB/s"))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Cpu:   "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("025.2% "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Mem:    "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("   097.6% "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Swap:  ") || s.Contains("Vir:   "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("   062.0% "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Disk "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("     0.1 MB/s "))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("User:  "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("013.3% "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Total:  "))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains(" 0036.0GB "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Total: "))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains(" 0002.0GB "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Peak "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("     0.1 MB/s "))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Kernel "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("011.9% "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Used:   "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains(" 0035.1GB "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Used:  "))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains(" 0001.2GB "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Idle:  "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("074.8% "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Free:   "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains(" 0000.9GB "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Free:  "))), Times.AtLeastOnce);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains(" 0000.8GB "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Proc:  "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("   739 "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Threads "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("     4993 "))), Times.Once);
        runContextHelper.terminal.Verify(t => t.Write(It.Is<string>(s => s.Contains("Ghosts "))), Times.Once);
        
        ctrl.Unload();
        MockInvocationsHelper.WriteInvocations(runContextHelper.terminal.Invocations, outputHelper);
    }
}
