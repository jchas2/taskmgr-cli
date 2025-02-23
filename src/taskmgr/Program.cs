using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using Task.Manager.System;
using Task.Manager.Cli.Utils;
using Task.Manager.Internal.Abstractions;
using Task.Manager.System.Process;

namespace Task.Manager;

class Program
{
    private const int UnhandledExceptionExitCode = 1;
    private const int DebugWait = 3000;
    
    private static void HandleException(UnhandledExceptionEventArgs ev)
    {
        if (ev.IsTerminating) {
            OutputWriter.Error.WriteLine(
                Environment.NewLine + "Runtime has encountered a fatal unhandled Exception.".ToRed());
        }
        
        if (ev.ExceptionObject is Exception e) {
            OutputWriter.Error.WriteLine(e.GetType().ToString().ToRed());
            OutputWriter.Error.WriteLine(e.Message.ToRed());
            OutputWriter.Error.WriteLine(e.StackTrace?? e.ToString().ToYellow());
            return;
        }
        
        OutputWriter.Error.WriteLine($"Unhandled error: {ev.ExceptionObject.GetType().Name}".ToRed());
    }
    
    private static int Main(string[] args)
    {
        using TerminalUTF8Encoder _ = new();
        Console.OutputEncoding = Encoding.UTF8;

        AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => {
            HandleException(eventArgs);
        };

        if (args.Any(arg => arg.Equals("--debug", StringComparison.CurrentCultureIgnoreCase))) {
            OutputWriter.Out.WriteLine($"Waiting for debugger attach to Pid {Environment.ProcessId}");
            while (false == Debugger.IsAttached) {
                Thread.Sleep(DebugWait);
            }
            Debugger.Break();
        }
        
        try {
            var runContext = new RunContext(
                new FileSystem(),
                new SystemInfo(),
                new Processes(),
                outputWriter: null);

            var app = new TaskMgrApp(runContext);
            return app.Run(args);
        }
        catch (Exception e) {
            HandleException(new UnhandledExceptionEventArgs(e, isTerminating: true));
            return UnhandledExceptionExitCode;
        }
    }
}
