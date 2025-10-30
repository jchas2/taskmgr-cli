using Task.Manager.Cli.Utils;
using Task.Manager.Internal.Abstractions;
using Task.Manager.System;
using Task.Manager.System.Process;
using IProcessor = Task.Manager.Process.IProcessor;

namespace Task.Manager;

public class RunContext(
    IFileSystem fileSystem,
    IProcessService processService,
    IModuleService moduleService,
    IThreadService threadService,
    IProcessor processor,
    IOutputWriter? outputWriter = null)
{
    public IFileSystem FileSystem { get; } = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    public IProcessService ProcessService { get; } = processService ?? throw new ArgumentNullException(nameof(processService));
    public IModuleService ModuleService { get; } = moduleService ?? throw new ArgumentNullException(nameof(moduleService));
    public IThreadService ThreadService { get; } = threadService ?? throw new ArgumentNullException(nameof(threadService));
    public IProcessor Processor { get; } = processor ?? throw new ArgumentNullException(nameof(processor));
    public IOutputWriter OutputWriter { get; } = outputWriter ?? Cli.Utils.OutputWriter.Out;
}
