using Moq;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.Internal.Abstractions;
using Task.Manager.Process;
using Task.Manager.System;
using Task.Manager.System.Process;

namespace Task.Manager.Tests;

public class RunContextTests
{
    [Fact]
    public void Should_Create_RunContext()
    {
        Mock<IFileSystem> fileSystem = new();
        Mock<ISystemTerminal> terminal = new();
        Mock<IProcessService> processService = new();
        Mock<IGpuService> gpuService = new();
        Mock<IModuleService> moduleService = new();
        Mock<IThreadService> threadService = new();
        Mock<IProcessor> processor = new();
        Mock<IOutputWriter> outputWriter = new();
        AppConfig appConfig = new(fileSystem.Object);
        
        RunContext context = new(
            fileSystem.Object,
            terminal.Object,
            processService.Object,
            gpuService.Object,
            moduleService.Object,
            threadService.Object,
            processor.Object,
            appConfig,
            outputWriter.Object);
        
        Assert.True(context.FileSystem == fileSystem.Object);
        Assert.True(context.Terminal == terminal.Object);
        Assert.True(context.ProcessService == processService.Object);
        Assert.True(context.GpuService == gpuService.Object);
        Assert.True(context.ModuleService == moduleService.Object);
        Assert.True(context.ThreadService == threadService.Object);
        Assert.True(context.Processor == processor.Object);
        Assert.True(context.AppConfig == appConfig);
        Assert.True(context.OutputWriter == outputWriter.Object);
    }
}
