using System.Diagnostics;
using System.Text;
using Task.Manager.System.Process;
using System.CommandLine;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.Cli.Utils;

namespace Task.Manager;

class Program
{
    private const string MutexId = "Task-Mgr-d3f8e2a1-4b6f-4e8a-9b2d-1c3e4f5a6b7c";
    
    private static void InitConfig()
    {
        
    }

    private static Option[] InitOptions()
    {
        Option[] opts = {
            new Option<int>(
                name: "--pid", 
                description: "Monitor the given PID.", 
                getDefaultValue: () => -1),
            new Option<string>("--username", "Monitor processes for the given username"),
            new Option<string>("--process", "Monitor processes matching or partially matching the given process name"),
            new Option<Statistics>(
                name: "--sort",
                description: "Sort the process display by sorting on the statistics column in descending order.",
                getDefaultValue: () => Statistics.Cpu),
            new Option<string>("--ascending", "Sort the statistics column in ascending order."),
            new Option<int>(
                name: "--iterations", 
                description: "Maximum number of iterations to execute before exiting.",
                getDefaultValue: () => -1),
            new Option<int>(
                name: "--nprocs",
                description: "Only display up to nprocs processes",
                getDefaultValue: () => -1),
        };

        return opts;
    }
    
    static int Main(string[] args)
    {
        using TerminalUTF8Encoder _ = new();
        Console.OutputEncoding = Encoding.UTF8;

        if (false == SystemInfo.IsRunningAsRoot()) {
            OutputWriter.Error.WriteLine("Application must be run as root user.".ToRed());
            return -1;
        }
        
        using var mutex = new Mutex(initiallyOwned: false, name: MutexId);
        
        if (false == mutex.WaitOne(TimeSpan.Zero, exitContext: false)) {
            OutputWriter.Error.WriteLine("Another instance of app is already running.".ToRed());
            return -1;
        }

        var options = InitOptions();

        return 0;
    }
}