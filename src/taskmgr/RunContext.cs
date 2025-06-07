using Task.Manager.Cli.Utils;
using Task.Manager.Internal.Abstractions;
using Task.Manager.System;
using Task.Manager.System.Process;

namespace Task.Manager;

public class RunContext(
    IFileSystem fileSystem,
    ISystemInfo systemInfo,
    IProcessor processor,
    IOutputWriter? outputWriter = null)
{
    public IFileSystem FileSystem { get; } = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    public IProcessor Processor { get; } = processor ?? throw new ArgumentNullException(nameof(processor));
    public ISystemInfo SystemInfo { get; } = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
    public IOutputWriter OutputWriter { get; } = outputWriter ?? Cli.Utils.OutputWriter.Out;
}
