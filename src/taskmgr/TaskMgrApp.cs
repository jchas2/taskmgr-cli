using System.CommandLine;
using System.Data;
using System.Reflection;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System.Configuration;

namespace Task.Manager;

public sealed class TaskMgrApp
{
    private const string MutexId = "Task-Mgr-d3f8e2a1-4b6f-4e8a-9b2d-1c3e4f5a6b7c";
    private const string ConfigFile = "taskmgr.config";

    private readonly RunContext _runContext;
    
    public TaskMgrApp(RunContext runContext)
    {
        _runContext = runContext ?? throw new ArgumentNullException(nameof(runContext));
    }
    
    private RootCommand InitRootCommand(Config config)
    {
        var pidOption = new Option<int?>(
            name: "--pid",
            description: "Monitor the given PID.");
        var usernameOption = new Option<string>(
            "--username", 
            "Monitor processes for the given username.");
        var processOption = new Option<string>(
            "--process", 
            "Monitor processes matching or partially matching the given process name.");
        var sortOption = new Option<Statistics?>(
            name: "--sort",
            description: "Sort the process display by sorting on the statistics column in descending order.");
        var ascendingOption = new Option<bool?>(
            name: "--ascending", 
            description: "Sort the statistics column in ascending order.");
        var limitOption = new Option<int?>(
            name: "--limit",
            description: "Limit the number of iterations to execute before exiting.");
        var nprocsOption = new Option<int?>(
            name: "--nprocs",
            description: "Only display up to nprocs processes.");
        var themeOption = new Option<string>(
            name: "--theme",
            description: "Load a theme from the config file. Default themes are \"colour\" and \"mono\".");
        var debugOption = new Option<bool?>(
            name: "--debug",
            description: "Pause execution on startup until a debugger is attached to the process.");
        
        var rootCommand = new RootCommand("Task Manager for the command line.") {
            pidOption,
            usernameOption,
            processOption,
            sortOption,
            ascendingOption,
            limitOption,
            nprocsOption,
            themeOption,
            debugOption,
        };
        
        rootCommand.SetHandler(context =>
        {
            /*
             * Map the incoming command line arguments to the current config instance.
             * Only override what's in config if a value comes in from the command line.
             */
            int? pid = context.ParseResult.GetValueForOption(pidOption);
            string? userName = context.ParseResult.GetValueForOption(usernameOption);
            string? process = context.ParseResult.GetValueForOption(processOption);
            Statistics? sortColumn = context.ParseResult.GetValueForOption(sortOption);
            bool? sortAscending = context.ParseResult.GetValueForOption(ascendingOption);
            int? limit = context.ParseResult.GetValueForOption(limitOption);
            int? nprocs = context.ParseResult.GetValueForOption(nprocsOption);
            string? themeName = context.ParseResult.GetValueForOption(themeOption);

            var filterSection = config.Sections.FirstOrDefault(s => 
                s.Name.Equals(Constants.Sections.Filter, StringComparison.CurrentCultureIgnoreCase));

            if (pid.HasValue && pid.Value >= 0) {
                filterSection?.Add(Constants.Keys.Pid, pid.Value.ToString());
            }

            if (false == string.IsNullOrWhiteSpace(userName)) {
                filterSection?.Add(Constants.Keys.UserName, userName);
            }

            if (false == string.IsNullOrWhiteSpace(process)) {
                filterSection?.Add(Constants.Keys.Process, process);
            }

            var sortSection = config.Sections.FirstOrDefault(s =>
                s.Name.Equals(Constants.Sections.Sort, StringComparison.CurrentCultureIgnoreCase));

            if (sortColumn.HasValue) {
                sortSection?.Add(Constants.Keys.Col, sortColumn.Value.ToString());
            }

            if (sortAscending.HasValue) {
                sortSection?.Add(Constants.Keys.Asc, sortAscending.Value.ToString());
            }

            var iterationsSection = config.Sections.FirstOrDefault(s =>
                s.Name.Equals(Constants.Sections.Iterations, StringComparison.CurrentCultureIgnoreCase));

            if (limit.HasValue && limit.Value >= 0) {
                iterationsSection?.Add(Constants.Keys.Limit, limit.Value.ToString());
            }

            var statsSection = config.Sections.FirstOrDefault(s =>
                s.Name.Equals(Constants.Sections.Stats, StringComparison.CurrentCultureIgnoreCase));

            if (nprocs.HasValue && nprocs.Value > 0) {
                statsSection?.Add(Constants.Keys.NProcs, nprocs.Value.ToString());
            }

            var uxSection = config.Sections.FirstOrDefault(s =>
                s.Name.Equals(Constants.Sections.UX, StringComparison.CurrentCultureIgnoreCase));

            if (false == string.IsNullOrEmpty(themeName)) {
                uxSection?.Add(Constants.Keys.DefaultTheme, themeName);                
            }

            /*
             * Now we have a config that's either been loaded from disk or generated
             * through the ConfigBuilder with defaults, and has had any command line
             * args that override a setting applied.
             *
             * Now we ensure all settings exist in the config by performing a merge
             * against the config instance with the default settings from the
             * ConfigBuilder.
             */
            ConfigBuilder.Merge(config);
            var theme = new Theme(config);
            
            RunCommand(
                _runContext,
                config,
                theme);
        });   
        
        return rootCommand;
    }

    private static int RunCommand(
        RunContext context, 
        Config config, 
        Theme theme)
    {
        return 0;
    }
    
    public int Run(string[] args)
    {
        if (false == _runContext.SystemInfo.IsRunningAsRoot()) {
            OutputWriter.Error.WriteLine("Application must be run as root user.".ToRed());
            return -1;
        }
        
        using var mutex = new Mutex(initiallyOwned: false, name: MutexId);
        
        if (false == mutex.WaitOne(timeout: TimeSpan.Zero, exitContext: false)) {
            OutputWriter.Error.WriteLine("Another instance of app is already running.".ToRed());
            return -1;
        }

        Config? config = null;
        
        if (TryGetConfigurationPath(out string? configPath) && false == string.IsNullOrEmpty(configPath)) {
            string configFile = Path.Combine(configPath, ConfigFile);
            if (_runContext.FileSystem.Exists(configFile)) {
                TryGetConfigurationFromFile(configFile, out config);
            }            
        }

        config ??= ConfigBuilder.BuildDefault();
        
        var rootCommand = InitRootCommand(config);
        int exitCode = rootCommand.Invoke(args);

        return exitCode;
    }

    private bool TryGetConfigurationFromFile(string configFile, out Config? config)
    {
        config = null;

        try {
            config = Config.FromFile(_runContext.FileSystem, configFile);
            return true;
        }
        catch (Exception e) when (e is FileNotFoundException || e is IOException) {
            _runContext.OutputWriter.WriteLine($"Error loading config: ${e.Message}.".ToRed());
        }
        catch (Exception e) when (e is ConfigParseException) {
            _runContext.OutputWriter.WriteLine($"Error parsing config: {e.Message}.".ToRed());
        }

        return false;
    }
    
    private bool TryGetConfigurationPath(out string? configPath)
    {
        configPath = string.Empty;
        
        try {
            configPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return true;
        }
        catch (Exception e) when (e is ArgumentException || e is PathTooLongException || e is IOException) {
            _runContext.OutputWriter.WriteLine($"Unable to get working path: {e.Message}.".ToRed());
            return false;
        }
    }
}