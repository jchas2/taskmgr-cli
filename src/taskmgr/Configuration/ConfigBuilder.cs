using System.Drawing;
using System.Net.Sockets;
using Task.Manager.System.Configuration;

namespace Task.Manager.Configuration;

public static class ConfigBuilder
{
    private const string StatsCols = "pid, process, user, pri, cpu, mem, virt, thrd, disk";
    
    public static Config BuildDefault() =>
        new Config()
            .AddConfigSection(BuildConfigSection(Constants.Sections.Filter))
            .AddConfigSection(BuildConfigSection(Constants.Sections.UX))
            .AddConfigSection(BuildConfigSection(Constants.Sections.Stats))
            .AddConfigSection(BuildConfigSection(Constants.Sections.Sort))
            .AddConfigSection(BuildConfigSection(Constants.Sections.Iterations))
            .AddConfigSection(BuildConfigSection(Constants.Sections.ThemeColour))
            .AddConfigSection(BuildConfigSection(Constants.Sections.ThemeMono));

    public static ConfigSection BuildConfigSection(string name)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        
        name = name.ToLower();
        
        switch (name) {
            case Constants.Sections.Filter:
                return new ConfigSection(Constants.Sections.Filter)
                    .Add(Constants.Keys.Pid, "-1")
                    .Add(Constants.Keys.UserName, string.Empty)
                    .Add(Constants.Keys.Process, string.Empty);
            case Constants.Sections.UX:
                return new ConfigSection(Constants.Sections.UX)
                    .Add(Constants.Keys.Bars, "false")
                    .Add(Constants.Keys.DefaultTheme, Constants.Sections.ThemeColour);
            case Constants.Sections.Stats:
                return new ConfigSection(Constants.Sections.Stats)
                    /* These cols must match the Configuration.Statistics enum members. */
                    .Add(Constants.Keys.Cols, StatsCols)
                    .Add(Constants.Keys.NProcs, "-1");
            case Constants.Sections.Sort:
                return new ConfigSection(Constants.Sections.Sort)
                    .Add(Constants.Keys.Col, "pid")
                    .Add(Constants.Keys.Asc, "false");
            case Constants.Sections.Iterations:
                return new ConfigSection(Constants.Sections.Iterations)
                    /* Max number of iterations to execute before exiting. <= 0 infinite. */
                    .Add(Constants.Keys.Limit, "0");
            case Constants.Sections.ThemeColour:
                return new ConfigSection(Constants.Sections.ThemeColour)
                    .Add(Constants.Keys.Background, "black")
                    .Add(Constants.Keys.BackgroundHighlight, "cyan")
                    .Add(Constants.Keys.Error, "red")
                    .Add(Constants.Keys.Foreground, "white")
                    .Add(Constants.Keys.ForegroundHighlight, "black")
                    .Add(Constants.Keys.Menubar, "gray")
                    .Add(Constants.Keys.RangeHighBackground, "red")
                    .Add(Constants.Keys.RangeLowBackground, "darkgreen")
                    .Add(Constants.Keys.RangeMidBackground, "darkyellow")
                    .Add(Constants.Keys.RangeHighForeground, "white")
                    .Add(Constants.Keys.RangeLowForeground, "black")
                    .Add(Constants.Keys.RangeMidForeground, "black")
                    .Add(Constants.Keys.HeaderBackground, "darkgreen")
                    .Add(Constants.Keys.HeaderForeground, "black");
            case Constants.Sections.ThemeMono:
                return new ConfigSection(Constants.Sections.ThemeMono)
                    .Add(Constants.Keys.Background, "black")
                    .Add(Constants.Keys.BackgroundHighlight, "white")
                    .Add(Constants.Keys.Error, "gray")
                    .Add(Constants.Keys.Foreground, "white")
                    .Add(Constants.Keys.ForegroundHighlight, "black")
                    .Add(Constants.Keys.Menubar, "gray")
                    .Add(Constants.Keys.RangeHighBackground, "gray")
                    .Add(Constants.Keys.RangeLowBackground, "white")
                    .Add(Constants.Keys.RangeMidBackground, "darkgray")
                    .Add(Constants.Keys.RangeHighForeground, "white")
                    .Add(Constants.Keys.RangeLowForeground, "white")
                    .Add(Constants.Keys.RangeMidForeground, "black")
                    .Add(Constants.Keys.HeaderBackground, "darkgray")
                    .Add(Constants.Keys.HeaderForeground, "white");
            default:
                throw new InvalidOperationException();
        }
    }
    
    private static ConfigSection GetConfigSection(string name, Config config) =>
        config.ContainsSection(name)
            ? config.GetConfigSection(name)
            : config.AddConfigSection(BuildConfigSection(name))
                .GetConfigSection(name);

    private static void MapColours(ConfigSection section, string[,] colourMap)
    {
        for (int i = 0; i < colourMap.GetLength(dimension: 0); i++) {
            section.AddIfMissing(colourMap[i, 0], colourMap[i, 1]);
        }
    }
    
    public static void Merge(Config withConfig)
    {
        ArgumentNullException.ThrowIfNull(withConfig, nameof(withConfig));
        
        /* Merge any changes required from a default config to ensure withConfig is a valid Config instance */
        ConfigSection filterSection = GetConfigSection(Constants.Sections.Filter, withConfig)
            .AddIfMissing(Constants.Keys.Pid, "-1")
            .AddIfMissing(Constants.Keys.UserName, string.Empty)
            .AddIfMissing(Constants.Keys.Process, string.Empty);

        ConfigSection uxSection = GetConfigSection(Constants.Sections.UX, withConfig)
            .AddIfMissing(Constants.Keys.Bars, "false")
            .AddIfMissing(Constants.Keys.DefaultTheme, Constants.Sections.ThemeColour);

        ConfigSection statsSection = GetConfigSection(Constants.Sections.Stats, withConfig)
            .AddIfMissing(Constants.Keys.Cols, StatsCols)
            .AddIfMissing(Constants.Keys.NProcs, "-1");

        ConfigSection sortSection = GetConfigSection(Constants.Sections.Sort, withConfig)
            .AddIfMissing(Constants.Keys.Col, "pid")
            .AddIfMissing(Constants.Keys.Asc, "false");

        ConfigSection iterSection = GetConfigSection(Constants.Sections.Iterations, withConfig)
            .AddIfMissing(Constants.Keys.Limit, "0");

        string[,] colourMap = {
            { Constants.Keys.Background, "black" },
            { Constants.Keys.BackgroundHighlight, "black" },
            { Constants.Keys.Error, "red" },
            { Constants.Keys.Foreground, "darkgray" },
            { Constants.Keys.ForegroundHighlight, "white" },
            { Constants.Keys.Menubar, "gray" },
            { Constants.Keys.RangeHighBackground, "red" },
            { Constants.Keys.RangeLowBackground, "darkgreen" },
            { Constants.Keys.RangeMidBackground, "darkyellow" },
            { Constants.Keys.HeaderBackground, "cyan" },
            { Constants.Keys.HeaderForeground, "white" }
        };

        string[,] monoMap = {
            { Constants.Keys.Background, "black" },
            { Constants.Keys.BackgroundHighlight, "black" },
            { Constants.Keys.Error, "gray" },
            { Constants.Keys.Foreground, "darkgray" },
            { Constants.Keys.ForegroundHighlight, "white" },
            { Constants.Keys.Menubar, "gray" },
            { Constants.Keys.RangeHighBackground, "gray" },
            { Constants.Keys.RangeLowBackground, "white" },
            { Constants.Keys.RangeMidBackground, "darkgray" },
            { Constants.Keys.HeaderBackground, "darkgray" },
            { Constants.Keys.HeaderForeground, "white" }
        };
        
        string defaultThemeName = uxSection.GetString(Constants.Keys.DefaultTheme, string.Empty);

        if (!string.IsNullOrWhiteSpace(defaultThemeName)) {
            switch (defaultThemeName) {
                case Constants.Sections.ThemeColour: {
                    ConfigSection colourSection = GetConfigSection(Constants.Sections.ThemeColour, withConfig);
                    MapColours(colourSection, colourMap);
                    break;
                }
                case Constants.Sections.ThemeMono: {
                    ConfigSection monoSection = GetConfigSection(Constants.Sections.ThemeMono, withConfig);
                    MapColours(monoSection, monoMap);
                    break;
                }
                default: {
                    ConfigSection themeSection = withConfig.ContainsSection(defaultThemeName)
                        ? withConfig.GetConfigSection(defaultThemeName)
                        : BuildConfigSection(Constants.Sections.ThemeColour);
                
                    /* When merging a custom theme, use the default colour map for any missing keys. */
                    MapColours(themeSection, colourMap);
                    break;
                }
            }
        }
        else {
            uxSection.AddIfMissing(Constants.Keys.DefaultTheme, Constants.Sections.ThemeColour);
            ConfigSection colourSection = GetConfigSection(Constants.Sections.ThemeColour, withConfig);
            MapColours(colourSection, colourMap);
        }
    }
}