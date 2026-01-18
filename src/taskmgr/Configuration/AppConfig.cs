using Task.Manager.Cli.Utils;
using Task.Manager.Gui.Controls;
using Task.Manager.Internal.Abstractions;
using Task.Manager.Process;
using Task.Manager.System.Configuration;

namespace Task.Manager.Configuration;

public sealed class AppConfig
{
    private readonly IFileSystem fileSystem;
    private Config iniConfig;
    private Theme defaultTheme = new();
    private readonly List<Theme> allThemes = new();

    private ConfigSection? filterSection;
    private ConfigSection? iterationSection;
    private ConfigSection? sortSection;
    private ConfigSection? statsSection;
    private ConfigSection? uxSection;
    
    private const string ConfigFile = "taskmgr.ini";
    
    private readonly string[,] colourMap = {
        { Constants.Keys.Background,            "black"       },
        { Constants.Keys.BackgroundHighlight,   "cyan"        },
        { Constants.Keys.ColCmdNormalUserSpace, "green"       },
        { Constants.Keys.ColCmdLowPriority,     "blue"        },
        { Constants.Keys.ColCmdHighCpu,         "red"         },
        { Constants.Keys.ColCmdIoBound,         "cyan"        },
        { Constants.Keys.ColCmdScript,          "yellow"      },
        { Constants.Keys.ColUserCurrentNonRoot, "green"       },
        { Constants.Keys.ColUserOtherNonRoot,   "magenta"     },
        { Constants.Keys.ColUserSystem,         "gray"        },
        { Constants.Keys.ColUserRoot,           "white"       },
        { Constants.Keys.CommandForeground,     "black"       },
        { Constants.Keys.CommandBackground,     "cyan"        },
        { Constants.Keys.Error,                 "red"         },
        { Constants.Keys.Foreground,            "white"       },
        { Constants.Keys.ForegroundHighlight,   "black"       },
        { Constants.Keys.MenubarForeground,     "white"       },
        { Constants.Keys.MenubarBackground,     "darkblue"    },
        { Constants.Keys.RangeHighBackground,   "red"         },
        { Constants.Keys.RangeLowBackground,    "green"       },
        { Constants.Keys.RangeMidBackground,    "yellow"      },
        { Constants.Keys.RangeHighForeground,   "white"       },
        { Constants.Keys.RangeLowForeground,    "white"       },
        { Constants.Keys.RangeMidForeground,    "darkyellow"  },
        { Constants.Keys.HeaderBackground,      "darkgreen"   },
        { Constants.Keys.HeaderForeground,      "black"       }};

    private readonly string[,] monoMap = {
        { Constants.Keys.Background,            "black"       },
        { Constants.Keys.BackgroundHighlight,   "darkgray"    },
        { Constants.Keys.ColCmdNormalUserSpace, "gray"        },
        { Constants.Keys.ColCmdLowPriority,     "darkgray"    },
        { Constants.Keys.ColCmdHighCpu,         "white"       },
        { Constants.Keys.ColCmdIoBound,         "white"       },
        { Constants.Keys.ColCmdScript,          "darkgray"    },
        { Constants.Keys.ColUserCurrentNonRoot, "darkgray"    },
        { Constants.Keys.ColUserOtherNonRoot,   "darkgray"    },
        { Constants.Keys.ColUserSystem,         "gray"        },
        { Constants.Keys.ColUserRoot,           "white"       },
        { Constants.Keys.CommandForeground,     "black"       },
        { Constants.Keys.CommandBackground,     "gray"        },
        { Constants.Keys.Error,                 "gray"        },
        { Constants.Keys.Foreground,            "darkgray"    },
        { Constants.Keys.ForegroundHighlight,   "white"       },
        { Constants.Keys.MenubarForeground,     "white"       },
        { Constants.Keys.MenubarBackground,     "gray"        },
        { Constants.Keys.RangeHighBackground,   "gray"        },
        { Constants.Keys.RangeLowBackground,    "gray"        },
        { Constants.Keys.RangeMidBackground,    "gray"        },
        { Constants.Keys.RangeHighForeground,   "darkgray"    },
        { Constants.Keys.RangeLowForeground,    "darkgray"    },
        { Constants.Keys.RangeMidForeground,    "darkgray"    },
        { Constants.Keys.HeaderBackground,      "darkgray"    },
        { Constants.Keys.HeaderForeground,      "white"       }};

    private readonly string[,] msDosMap = {
        { Constants.Keys.Background,            "darkblue"    },
        { Constants.Keys.BackgroundHighlight,   "cyan"        },
        { Constants.Keys.ColCmdNormalUserSpace, "yellow"      },
        { Constants.Keys.ColCmdLowPriority,     "gray"        },
        { Constants.Keys.ColCmdHighCpu,         "red"         },
        { Constants.Keys.ColCmdIoBound,         "red"         },
        { Constants.Keys.ColCmdScript,          "yellow"      },
        { Constants.Keys.ColUserCurrentNonRoot, "gray"        },
        { Constants.Keys.ColUserOtherNonRoot,   "darkgray"    },
        { Constants.Keys.ColUserSystem,         "yellow"      },
        { Constants.Keys.ColUserRoot,           "red"         },
        { Constants.Keys.CommandForeground,     "yellow"      },
        { Constants.Keys.CommandBackground,     "darkblue"    },
        { Constants.Keys.Error,                 "red"         },
        { Constants.Keys.Foreground,            "darkgrey"    },
        { Constants.Keys.ForegroundHighlight,   "black"       },
        { Constants.Keys.MenubarForeground,     "yellow"      },
        { Constants.Keys.MenubarBackground,     "blue"        },
        { Constants.Keys.RangeHighBackground,   "red"         },
        { Constants.Keys.RangeLowBackground,    "cyan"        },
        { Constants.Keys.RangeMidBackground,    "darkcyan"    },
        { Constants.Keys.RangeHighForeground,   "red"         },
        { Constants.Keys.RangeLowForeground,    "cyan"        },
        { Constants.Keys.RangeMidForeground,    "darkcyan"    },
        { Constants.Keys.HeaderBackground,      "darkblue"    },
        { Constants.Keys.HeaderForeground,      "yellow"      }};

    private readonly string[,] tokyoNightMap = {
        { Constants.Keys.Background,            "black"       },
        { Constants.Keys.BackgroundHighlight,   "cyan"        },
        { Constants.Keys.ColCmdNormalUserSpace, "darkgray"    },
        { Constants.Keys.ColCmdLowPriority,     "gray"        },
        { Constants.Keys.ColCmdHighCpu,         "red"         },
        { Constants.Keys.ColCmdIoBound,         "cyan"        },
        { Constants.Keys.ColCmdScript,          "yellow"      },
        { Constants.Keys.ColUserCurrentNonRoot, "yellow"      },
        { Constants.Keys.ColUserOtherNonRoot,   "magenta"     },
        { Constants.Keys.ColUserSystem,         "gray"        },
        { Constants.Keys.ColUserRoot,           "white"       },
        { Constants.Keys.CommandForeground,     "magenta"     },
        { Constants.Keys.CommandBackground,     "darkblue"    },
        { Constants.Keys.Error,                 "red"         },
        { Constants.Keys.Foreground,            "cyan"        },
        { Constants.Keys.ForegroundHighlight,   "darkmagenta" },
        { Constants.Keys.MenubarForeground,     "magenta"     },
        { Constants.Keys.MenubarBackground,     "darkblue"    },
        { Constants.Keys.RangeHighBackground,   "red"         },
        { Constants.Keys.RangeLowBackground,    "magenta"     },
        { Constants.Keys.RangeMidBackground,    "magenta"     },
        { Constants.Keys.RangeHighForeground,   "cyan"        },
        { Constants.Keys.RangeLowForeground,    "cyan"        },
        { Constants.Keys.RangeMidForeground,    "cyan"        },
        { Constants.Keys.HeaderBackground,      "blue"        },
        { Constants.Keys.HeaderForeground,      "magenta"     }};

    private readonly string[,] matrixMap = {
        { Constants.Keys.Background,            "black"       },
        { Constants.Keys.BackgroundHighlight,   "green"       },
        { Constants.Keys.ColCmdNormalUserSpace, "green"       },
        { Constants.Keys.ColCmdLowPriority,     "darkgreen"   },
        { Constants.Keys.ColCmdHighCpu,         "green"       },
        { Constants.Keys.ColCmdIoBound,         "green"       },
        { Constants.Keys.ColCmdScript,          "darkgreen"   },
        { Constants.Keys.ColUserCurrentNonRoot, "darkgreen"   },
        { Constants.Keys.ColUserOtherNonRoot,   "darkgreen"   },
        { Constants.Keys.ColUserSystem,         "gray"        },
        { Constants.Keys.ColUserRoot,           "green"       },
        { Constants.Keys.CommandForeground,     "black"       },
        { Constants.Keys.CommandBackground,     "darkgreen"   },
        { Constants.Keys.Error,                 "red"         },
        { Constants.Keys.Foreground,            "green"       },
        { Constants.Keys.ForegroundHighlight,   "black"       },
        { Constants.Keys.MenubarForeground,     "black"       },
        { Constants.Keys.MenubarBackground,     "darkgreen"   },
        { Constants.Keys.RangeHighBackground,   "darkgreen"   },
        { Constants.Keys.RangeLowBackground,    "green"       },
        { Constants.Keys.RangeMidBackground,    "darkgreen"   },
        { Constants.Keys.RangeHighForeground,   "black"       },
        { Constants.Keys.RangeLowForeground,    "black"       },
        { Constants.Keys.RangeMidForeground,    "darkgreen"   },
        { Constants.Keys.HeaderBackground,      "green"       },
        { Constants.Keys.HeaderForeground,      "black"       }};

    public AppConfig(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
        this.iniConfig = new();
        LoadSections();
    }

    public AppConfig(IFileSystem fileSystem, Config iniConfig)
    {
        this.fileSystem = fileSystem;
        this.iniConfig = iniConfig;
        LoadSections();
    }

    public bool ConfirmTaskDelete
    {
        get => uxSection?.GetBool(Constants.Keys.ConfirmTaskDelete, true) ?? true;
        set => uxSection?.Add(Constants.Keys.ConfirmTaskDelete, value.ToString());
    }

    public string? DefaultConfigPath
    {
        get {
            try {
                return Path.Combine(AppContext.BaseDirectory, ConfigFile);
            }
            catch (Exception ex) {
                ExceptionHelper.HandleException(ex);
                return null;
            }
        }
    }
    
    public Theme DefaultTheme
    {
        get => defaultTheme;
        set {
            if (!allThemes.Contains(value)) {
                throw new InvalidOperationException();
            }

            defaultTheme = value;
            
            if (iniConfig.ConfigSections.Any(cs => cs.Name.Equals(value.Name, StringComparison.CurrentCultureIgnoreCase))) {
                uxSection?.Add(Constants.Keys.DefaultTheme, value.Name);
            }
        }
    }
    
    public int DelayInMilliseconds
    {
        get => statsSection?.GetInt(Constants.Keys.Delay, Processor.DefaultDelayInMilliseconds) ??
               Processor.DefaultDelayInMilliseconds;
        set => statsSection?.Add(Constants.Keys.Delay, value.ToString());
    }

    public int FilterPid
    {
        get => filterSection?.GetInt(Constants.Keys.Pid, -1) ?? -1;
        set => filterSection?.Add(Constants.Keys.Pid, value.ToString());
    }

    public string FilterUserName
    {
        get => filterSection?.GetString(Constants.Keys.UserName, string.Empty) ?? string.Empty;
        set => filterSection?.Add(Constants.Keys.UserName, value);
    }

    public string FilterProcess
    {
        get => filterSection?.GetString(Constants.Keys.Process, string.Empty) ?? string.Empty;
        set => filterSection?.Add(Constants.Keys.Process, value);
    }
    
    public bool HighlightDaemons
    {
        get => uxSection?.GetBool(Constants.Keys.HighlightDaemons, true) ?? true;
        set => uxSection?.Add(Constants.Keys.HighlightDaemons, value.ToString());
    }
    
    public bool HighlightStatisticsColumnUpdate
    {
        get => uxSection?.GetBool(Constants.Keys.HighlightStatsColUpdate, true) ?? true;
        set => uxSection?.Add(Constants.Keys.HighlightStatsColUpdate, value.ToString());
    }

    public MetreControlStyle MetreStyle
    {
        get => uxSection?.GetEnum(Constants.Keys.MetreStyle, MetreControlStyle.Dots) ?? MetreControlStyle.Dots;
        set => uxSection?.Add(Constants.Keys.MetreStyle, value.ToString());
    }
    
    public bool MultiSelectProcesses
    {
        get => uxSection?.GetBool(Constants.Keys.MultiSelectProcesses, false) ?? false;
        set => uxSection?.Add(Constants.Keys.MultiSelectProcesses, value.ToString());
    }

    public int NumberOfProcesses
    {
        get => statsSection?.GetInt(Constants.Keys.NProcs, -1) ?? -1;
        set => statsSection?.Add(Constants.Keys.NProcs, value.ToString());
    }

    public Statistics SortColumn
    {
        get => sortSection?.GetEnum(Constants.Keys.Col, Statistics.Cpu) ?? Statistics.Cpu;
        set => sortSection?.Add(Constants.Keys.Col, value.ToString());
    }

    public bool SortAscending
    {
        get => sortSection?.GetBool(Constants.Keys.Asc, false) ?? false;
        set => sortSection?.Add(Constants.Keys.Asc, value.ToString());
    }

    public int IterationLimit
    {
        get => iterationSection?.GetInt(Constants.Keys.Limit, 0) ?? 0;
        set => iterationSection?.Add(Constants.Keys.Limit, value.ToString());
    }
    
    public bool ShowMetreCpuNumerically
    {
        get => uxSection?.GetBool(Constants.Keys.ShowMetreCpuNumerically, true) ?? true;
        set => uxSection?.Add(Constants.Keys.ShowMetreCpuNumerically, value.ToString());
    }

    public bool ShowMetreDiskNumerically
    {
        get => uxSection?.GetBool(Constants.Keys.ShowMetreDiskNumerically, true) ?? true;
        set => uxSection?.Add(Constants.Keys.ShowMetreDiskNumerically, value.ToString());
    }

    public bool ShowMetreMemoryNumerically
    {
        get => uxSection?.GetBool(Constants.Keys.ShowMetreMemNumerically, true) ?? true;
        set => uxSection?.Add(Constants.Keys.ShowMetreMemNumerically, value.ToString());
    }

    public bool ShowMetreSwapNumerically
    {
        get => uxSection?.GetBool(Constants.Keys.ShowMetreSwapNumerically, true) ?? true;
        set => uxSection?.Add(Constants.Keys.ShowMetreSwapNumerically, value.ToString());
    }

    public bool UseIrixReporting
    {
#if __WIN32__
        get => uxSection?.GetBool(Constants.Keys.UseIrixCpuReporting, false) ?? false;
        set => uxSection?.Add(Constants.Keys.UseIrixCpuReporting, value.ToString());
#endif
#if __APPLE__        
        get => uxSection?.GetBool(Constants.Keys.UseIrixCpuReporting, true) ?? true;
        set => uxSection?.Add(Constants.Keys.UseIrixCpuReporting, value.ToString());
#endif
    }

    private void LoadSections()
    {
        filterSection = iniConfig.ContainsSection(Constants.Sections.Filter)
            ? iniConfig.GetConfigSection(Constants.Sections.Filter)
            : new ConfigSection(Constants.Sections.Filter);

        filterSection
            .AddIfMissing(Constants.Keys.Pid, "-1")
            .AddIfMissing(Constants.Keys.UserName, string.Empty)
            .AddIfMissing(Constants.Keys.Process, string.Empty);

        if (!iniConfig.ContainsSection(filterSection.Name)) {
            iniConfig.AddConfigSection(filterSection);
        }

        iterationSection = iniConfig.ContainsSection(Constants.Sections.Iterations)
            ? iniConfig.GetConfigSection(Constants.Sections.Iterations)
            : new ConfigSection(Constants.Sections.Iterations);

        iterationSection.AddIfMissing(Constants.Keys.Limit, "0");

        if (!iniConfig.ContainsSection(iterationSection.Name)) {
            iniConfig.AddConfigSection(iterationSection);
        }

        sortSection = iniConfig.ContainsSection(Constants.Sections.Sort)
            ? iniConfig.GetConfigSection(Constants.Sections.Sort)
            : new ConfigSection(Constants.Sections.Sort);

        sortSection
            .AddIfMissing(Constants.Keys.Col, Statistics.Cpu.ToString())
            .AddIfMissing(Constants.Keys.Asc, false.ToString());

        if (!iniConfig.ContainsSection(sortSection.Name)) {
            iniConfig.AddConfigSection(sortSection);
        }
        
        statsSection = iniConfig.ContainsSection(Constants.Sections.Stats)
            ? iniConfig.GetConfigSection(Constants.Sections.Stats)
            : new ConfigSection(Constants.Sections.Stats);

        statsSection
            .AddIfMissing(Constants.Keys.Cols, string.Join(", ", Enum.GetNames<Statistics>()))
            .AddIfMissing(Constants.Keys.Delay, Processor.DefaultDelayInMilliseconds.ToString())
            .AddIfMissing(Constants.Keys.NProcs, "-1");

        if (!iniConfig.ContainsSection(statsSection.Name)) {
            iniConfig.AddConfigSection(statsSection);
        }
        
        uxSection = iniConfig.ContainsSection(Constants.Sections.UX)
            ? iniConfig.GetConfigSection(Constants.Sections.UX)
            : new ConfigSection(Constants.Sections.UX);

        uxSection
            .AddIfMissing(Constants.Keys.ConfirmTaskDelete, true.ToString())
            .AddIfMissing(Constants.Keys.DefaultTheme, Constants.Sections.ThemeColour)
            .AddIfMissing(Constants.Keys.HighlightDaemons, true.ToString())
            .AddIfMissing(Constants.Keys.HighlightStatsColUpdate, true.ToString())
            .AddIfMissing(Constants.Keys.MetreStyle, MetreControlStyle.Dots.ToString())
            .AddIfMissing(Constants.Keys.MultiSelectProcesses, false.ToString())
            .AddIfMissing(Constants.Keys.ShowMetreCpuNumerically, true.ToString())
            .AddIfMissing(Constants.Keys.ShowMetreDiskNumerically, true.ToString())
            .AddIfMissing(Constants.Keys.ShowMetreMemNumerically, true.ToString())
            .AddIfMissing(Constants.Keys.ShowMetreSwapNumerically, true.ToString())
#if __WIN32__            
            .AddIfMissing(Constants.Keys.UseIrixCpuReporting, false.ToString());
#endif
#if __APPLE__
            .AddIfMissing(Constants.Keys.UseIrixCpuReporting, true.ToString());
#endif

        if (!iniConfig.ContainsSection(uxSection.Name)) {
            iniConfig.AddConfigSection(uxSection);
        }
        
        var themeMaps = new Dictionary<string, string[,]> { 
            [Constants.Sections.ThemeColour] = colourMap,
            [Constants.Sections.ThemeMono] = monoMap,
            [Constants.Sections.ThemeMsDos] = msDosMap,
            [Constants.Sections.ThemeTokyoNight] = tokyoNightMap,
            [Constants.Sections.ThemeMatrix] = matrixMap
        };

        foreach (string themeName in themeMaps.Keys) {
            if (!iniConfig.ContainsSection(themeName)) {
                ConfigSection themeSection = new(themeName);
                MapColours(themeSection, themeMaps[themeName]);
                iniConfig.AddConfigSection(themeSection);
            }
        }

        List<ConfigSection> themeSections = iniConfig.ConfigSections
            .Where(cs => cs.Name.StartsWith("theme-", StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        foreach (ConfigSection configSection in themeSections) {
            Theme? theme = allThemes.FirstOrDefault(t => t.Name.Equals(configSection.Name, StringComparison.CurrentCultureIgnoreCase));
            if (theme != null) {
                theme.Update(configSection);
            }
            else {
                allThemes.Add(new Theme(configSection));
            }
        }

        if (allThemes.Any(t => t.Name.Equals(uxSection.GetString(Constants.Keys.DefaultTheme), StringComparison.CurrentCultureIgnoreCase))) {
            defaultTheme = allThemes
                .Where(t => t.Name == uxSection.GetString(Constants.Keys.DefaultTheme))
                .First();
        }
    }

    private void MapColours(ConfigSection section, string[,] map)
    {
        for (int i = 0; i < map.GetLength(dimension: 0); i++) {
            section.AddIfMissing(map[i, 0], map[i, 1]);
        }
    }
    
    public List<Theme> Themes => allThemes;

    public bool TryLoad(Config config)
    {
        try {
            iniConfig = config;
            LoadSections();
            return true;
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex);
            return false;
        }
    }
    
    public bool TryLoad(string path)
    {
        try {
            iniConfig = Config.FromFile(fileSystem, path);
            LoadSections();
            return true;
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is IOException) {
            ExceptionHelper.HandleException(ex, $"Error loading config: ${ex.Message}.");
        }
        catch (Exception ex) when (ex is ConfigParseException) {
            ExceptionHelper.HandleException(ex, $"Error parsing config: {ex.Message}.");
        }

        return false;
    }

    public bool TrySave(string path)
    {
        try {
            Config.ToFile(fileSystem, path, iniConfig);
            return true;
        }
        catch (Exception ex) {
            ExceptionHelper.HandleException(ex, $"Error saving config: {ex.Message} to path {path}");
            return false;
        }
    }
}
