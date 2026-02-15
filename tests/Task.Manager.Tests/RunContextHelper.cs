using Moq;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.Internal.Abstractions;
using Task.Manager.Process;
using Task.Manager.System;
using Task.Manager.System.Process;

namespace Task.Manager.Tests;

internal class RunContextHelper
{
    // Internal for Mock Verification pattern.
    internal Mock<IFileSystem> fileSystem = new();
    internal Mock<ISystemTerminal> terminal = new();
    internal Mock<IProcessService> processService = new();
    internal Mock<IGpuService> gpuService = new();
    internal Mock<IModuleService> moduleService = new();
    internal Mock<IThreadService> threadService = new();
    internal Mock<IProcessor> processor = new();
    internal Mock<IOutputWriter> outputWriter = new();
    internal AppConfig appConfig;

    public RunContextHelper()
    {
        appConfig = new(fileSystem.Object);
        
        terminal.Setup(t => t.WindowHeight).Returns(32);
        terminal.Setup(t => t.WindowWidth).Returns(32);
        terminal.Setup(t => t.BackgroundColor).Returns(ConsoleColor.Black);
        terminal.Setup(t => t.ForegroundColor).Returns(ConsoleColor.White);
        terminal.Setup(t => t.CursorLeft).Returns(0);
        terminal.Setup(t => t.CursorTop).Returns(0);
        terminal.Setup(t => t.KeyAvailable).Returns(false);
    }

    internal RunContext GetRunContext() =>
        new RunContext(
            fileSystem.Object,
            terminal.Object,
            processService.Object,
            gpuService.Object,
            moduleService.Object,
            threadService.Object,
            processor.Object,
            appConfig,
            outputWriter.Object);
}