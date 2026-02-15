using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.Internal.Abstractions;
using Task.Manager.System;
using Task.Manager.System.Process;
using IProcessor = Task.Manager.Process.IProcessor;

namespace Task.Manager;

public class RunContext(
    IFileSystem fileSystem,
    ISystemTerminal terminal,
    IProcessService processService,
    IGpuService gpuService,
    IModuleService moduleService,
    IThreadService threadService,
    IProcessor processor,
    AppConfig appConfig,
    IOutputWriter? outputWriter = null)
{
    public IFileSystem FileSystem { get; } = fileSystem;
    public ISystemTerminal Terminal { get; } = terminal;
    public IProcessService ProcessService { get; } = processService;
    public IGpuService GpuService { get; } = gpuService;
    public IModuleService ModuleService { get; } = moduleService;
    public IThreadService ThreadService { get; } = threadService;
    public IProcessor Processor { get; } = processor;
    public AppConfig AppConfig { get; } = appConfig;
    public IOutputWriter OutputWriter { get; } = outputWriter ?? Cli.Utils.OutputWriter.Out;
}
