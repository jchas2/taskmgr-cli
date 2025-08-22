using System.CommandLine;
using System.Data;
using System.Reflection;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.Gui;
using Task.Manager.System;
using Task.Manager.System.Configuration;
using Task.Manager.System.Screens;

namespace Task.Manager;

public sealed class TaskMgrApp
{
    private const string MutexId = "Task-Mgr-d3f8e2a1-4b6f-4e8a-9b2d-1c3e4f5a6b7c";
    private const string ConfigFile = "taskmgr.config";

    private readonly RunContext runContext;
    private static Mutex? mutex = null;
    
    public TaskMgrApp(RunContext runContext)
    {
        this.runContext = runContext ?? throw new ArgumentNullException(nameof(runContext));
    }
    
    private RootCommand InitRootCommand(Config config)
    {
        Option<int?> pidOption = new(
            name: "--pid",
            description: "Monitor the given PID.");
        Option<string> usernameOption = new(
            "--username", 
            "Monitor processes for the given username.");
        Option<string> processOption = new(
            "--process", 
            "Monitor processes matching or partially matching the given process name.");
        Option<Statistics?> sortOption = new(
            name: "--sort",
            description: "Sort the process display by sorting on the statistics column in descending order.");
        Option<bool?> ascendingOption = new(
            name: "--ascending", 
            description: "Sort the statistics column in ascending order.");
        Option<int?> limitOption = new(
            name: "--limit",
            description: "Limit the number of iterations to execute before exiting.");
        Option<int?> nprocsOption = new(
            name: "--nprocs",
            description: "Only display up to nprocs processes.");
        Option<string> themeOption = new(
            name: "--theme",
            description: "Load a theme from the config file. Default themes are \"theme-colour\" and \"theme-mono\".");
        Option<bool?> debugOption = new(
            name: "--debug",
            description: "Pause execution on startup until a debugger is attached to the process.");
        
        RootCommand rootCommand = new("Task Manager for the command line.") {
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

            ConfigSection? filterSection = config.ConfigSections.FirstOrDefault(s => 
                s.Name.Equals(Constants.Sections.Filter, StringComparison.CurrentCultureIgnoreCase));

            if (pid.HasValue && pid.Value >= 0) {
                filterSection?.Add(Constants.Keys.Pid, pid.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(userName)) {
                filterSection?.Add(Constants.Keys.UserName, userName);
            }

            if (!string.IsNullOrWhiteSpace(process)) {
                filterSection?.Add(Constants.Keys.Process, process);
            }

            ConfigSection? sortSection = config.ConfigSections.FirstOrDefault(s =>
                s.Name.Equals(Constants.Sections.Sort, StringComparison.CurrentCultureIgnoreCase));

            if (sortColumn.HasValue) {
                sortSection?.Add(Constants.Keys.Col, sortColumn.Value.ToString());
            }

            if (sortAscending.HasValue) {
                sortSection?.Add(Constants.Keys.Asc, sortAscending.Value.ToString());
            }

            ConfigSection? iterationsSection = config.ConfigSections.FirstOrDefault(s =>
                s.Name.Equals(Constants.Sections.Iterations, StringComparison.CurrentCultureIgnoreCase));

            if (limit.HasValue && limit.Value >= 0) {
                iterationsSection?.Add(Constants.Keys.Limit, limit.Value.ToString());
            }

            ConfigSection? statsSection = config.ConfigSections.FirstOrDefault(s =>
                s.Name.Equals(Constants.Sections.Stats, StringComparison.CurrentCultureIgnoreCase));

            if (nprocs.HasValue && nprocs.Value > 0) {
                statsSection?.Add(Constants.Keys.NProcs, nprocs.Value.ToString());
            }

            ConfigSection? uxSection = config.ConfigSections.FirstOrDefault(s =>
                s.Name.Equals(Constants.Sections.UX, StringComparison.CurrentCultureIgnoreCase));

            if (!string.IsNullOrWhiteSpace(themeName)) {
                if (config.ContainsSection(themeName)) {
                    uxSection?.Add(Constants.Keys.DefaultTheme, themeName);    
                }
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
            Theme theme = new(config);
            
            RunCommand(
                runContext,
                config,
                theme);
        });   
        
        return rootCommand;
    }

    private static int RunCommand(
        RunContext runContext, 
        Config config, 
        Theme theme)
    {
        SystemTerminal terminal = new();
        
        MainScreen mainScreen = new(
            runContext, 
            terminal, 
            theme, 
            config);

        HelpScreen helpScreen = new(terminal);
        SetupScreen setupScreen = new(terminal);
        
        ScreenApplication.RegisterScreen(mainScreen);
        ScreenApplication.RegisterScreen(helpScreen);
        ScreenApplication.RegisterScreen(setupScreen);
        
        ScreenApplication.Run(mainScreen);
        
        return 0;
    }
    
    public int Run(string[] args)
    {
#if !DEBUG
        if (false == runContext.SystemInfo.IsRunningAsRoot()) {
            OutputWriter.Error.WriteLine("Application must be run as root user.".ToRed());
            return -1;
        }
#endif
        var createdMutex = true;

        try {
            mutex = new Mutex(initiallyOwned: false, name: MutexId, out createdMutex);

            if (!mutex.WaitOne(0, false)) {
                runContext.OutputWriter.WriteLine("Another instance of app is already running.".ToRed());
                return -1;
            }

            Config? config = null;

            if (TryGetConfigurationPath(out string? configPath) && !string.IsNullOrEmpty(configPath)) {
                string configFile = Path.Combine(configPath, ConfigFile);
                if (runContext.FileSystem.Exists(configFile)) {
                    TryGetConfigurationFromFile(configFile, out config);
                }
            }

            config ??= ConfigBuilder.BuildDefault();

            RootCommand rootCommand = InitRootCommand(config);
            int exitCode = rootCommand.Invoke(args);

            return exitCode;
        }
        finally {
            if (mutex != null) {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }

    private bool TryGetConfigurationFromFile(string configFile, out Config? config)
    {
        config = null;

        try {
            config = Config.FromFile(runContext.FileSystem, configFile);
            return true;
        }
        catch (Exception e) when (e is FileNotFoundException || e is IOException) {
            runContext.OutputWriter.WriteLine($"Error loading config: ${e.Message}.".ToRed());
        }
        catch (Exception e) when (e is ConfigParseException) {
            runContext.OutputWriter.WriteLine($"Error parsing config: {e.Message}.".ToRed());
        }

        return false;
    }
    
    private bool TryGetConfigurationPath(out string? configPath)
    {
        configPath = string.Empty;
        
        try {
            configPath = AppContext.BaseDirectory;
            return true;
        }
        catch (Exception e) when (e is ArgumentException || e is PathTooLongException || e is IOException) {
            runContext.OutputWriter.WriteLine($"Unable to get working path: {e.Message}.".ToRed());
            return false;
        }
    }
}