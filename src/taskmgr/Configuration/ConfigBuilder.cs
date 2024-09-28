using System.Drawing;
using Task.Manager.System.Configuration;

namespace Task.Manager.Configuration;

public static class ConfigBuilder
{
    private const string StatsCols = "pid, process, user, pri, cpu, mem, virt, thrd, disk";
    
    public static Config BuildDefault()
    {
        return new Config()
            .AddSection(BuildSection(Constants.Sections.Filter))
            .AddSection(BuildSection(Constants.Sections.UX))
            .AddSection(BuildSection(Constants.Sections.Stats))
            .AddSection(BuildSection(Constants.Sections.Sort))
            .AddSection(BuildSection(Constants.Sections.Iterations))
            .AddSection(BuildSection(Constants.Sections.ThemeColour))
            .AddSection(BuildSection(Constants.Sections.ThemeMono));
    }

    private static ConfigSection GetSection(string name, Config config) =>
        config.ContainsSection(name)
            ? config.GetSection(name)
            : config.AddSection(BuildSection(name))
                .GetSection(name);

    private static void MapColours(ConfigSection section, string[,] colourMap)
    {
        for (int i = 0; i < colourMap.GetLength(dimension: 0); i++) {
            section.AddIfMissing(colourMap[i, 0], colourMap[i, 1]);
        }
    }
    
    public static void Merge(Config withConfig)
    {
        /* Merge any changes required from a default config to ensure withConfig is a valid Config instance */
        var filterSection = GetSection(Constants.Sections.Filter, withConfig);
        filterSection.AddIfMissing(Constants.Keys.Pid, "-1");
        filterSection.AddIfMissing(Constants.Keys.UserName, string.Empty);
        filterSection.AddIfMissing(Constants.Keys.Process, string.Empty);

        var uxSection = GetSection(Constants.Sections.UX, withConfig);
        uxSection.AddIfMissing(Constants.Keys.Bars, "false");
        uxSection.AddIfMissing(Constants.Keys.DefaultTheme, Constants.Sections.ThemeColour);

        var statsSection = GetSection(Constants.Sections.Stats, withConfig);
        statsSection.AddIfMissing(Constants.Keys.Cols, StatsCols);
        statsSection.AddIfMissing(Constants.Keys.NProcs, "-1");

        var sortSection = GetSection(Constants.Sections.Sort, withConfig);
        sortSection.AddIfMissing(Constants.Keys.Col, "pid");
        sortSection.AddIfMissing(Constants.Keys.Asc, "false");

        var iterSection = GetSection(Constants.Sections.Iterations, withConfig);
        iterSection.AddIfMissing(Constants.Keys.Limit, "0");

        string[,] colourMap = {
            { Constants.Keys.Background, "black" },
            { Constants.Keys.BackgroundHighlight, "black" },
            { Constants.Keys.Error, "red" },
            { Constants.Keys.Foreground, "darkgray" },
            { Constants.Keys.ForegroundHighlight, "white" },
            { Constants.Keys.Menubar, "gray" },
            { Constants.Keys.RangeHigh, "red" },
            { Constants.Keys.RangeLow, "green" },
            { Constants.Keys.RangeMid, "yellow" },
            { Constants.Keys.ProcessHeader, "cyan" }
        };

        string[,] monoMap = {
            { Constants.Keys.Background, "black" },
            { Constants.Keys.BackgroundHighlight, "black" },
            { Constants.Keys.Error, "gray" },
            { Constants.Keys.Foreground, "darkgray" },
            { Constants.Keys.ForegroundHighlight, "white" },
            { Constants.Keys.Menubar, "gray" },
            { Constants.Keys.RangeHigh, "gray" },
            { Constants.Keys.RangeLow, "white" },
            { Constants.Keys.RangeMid, "darkgray" },
            { Constants.Keys.ProcessHeader, "darkgray" }
        };
        
        string defaultThemeName = uxSection.GetString(Constants.Keys.DefaultTheme, string.Empty);

        if (false == string.IsNullOrEmpty(defaultThemeName)) {
            switch (defaultThemeName) {
                case Constants.Sections.ThemeColour: {
                    var colourSection = GetSection(Constants.Sections.ThemeColour, withConfig);
                    MapColours(colourSection, colourMap);
                    break;
                }
                case Constants.Sections.ThemeMono: {
                    var monoSection = GetSection(Constants.Sections.ThemeMono, withConfig);
                    MapColours(monoSection, monoMap);
                    break;
                }
                default: {
                    var themeSection = withConfig.ContainsSection(defaultThemeName)
                        ? withConfig.GetSection(defaultThemeName)
                        : BuildSection(Constants.Sections.ThemeColour);
                
                    /* When merging a custom theme, use the default colour map for any missing keys. */
                    MapColours(themeSection, colourMap);
                    break;
                }
            }
        }
        else {
            uxSection.AddIfMissing(Constants.Keys.DefaultTheme, Constants.Sections.ThemeColour);
            var colourSection = GetSection(Constants.Sections.ThemeColour, withConfig);
            MapColours(colourSection, colourMap);
        }
    }

    public static ConfigSection BuildSection(string name)
    {
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
                    .Add(Constants.Keys.BackgroundHighlight, "black")
                    .Add(Constants.Keys.Error, "red")
                    .Add(Constants.Keys.Foreground, "darkgray")
                    .Add(Constants.Keys.ForegroundHighlight, "white")
                    .Add(Constants.Keys.Menubar, "gray")
                    .Add(Constants.Keys.RangeHigh, "red")
                    .Add(Constants.Keys.RangeLow, "green")
                    .Add(Constants.Keys.RangeMid, "yellow")
                    .Add(Constants.Keys.ProcessHeader, "cyan");
            case Constants.Sections.ThemeMono:
                return new ConfigSection(Constants.Sections.ThemeMono)
                    .Add(Constants.Keys.Background, "black")
                    .Add(Constants.Keys.BackgroundHighlight, "black")
                    .Add(Constants.Keys.Error, "gray")
                    .Add(Constants.Keys.Foreground, "darkgray")
                    .Add(Constants.Keys.ForegroundHighlight, "white")
                    .Add(Constants.Keys.Menubar, "gray")
                    .Add(Constants.Keys.RangeHigh, "gray")
                    .Add(Constants.Keys.RangeLow, "white")
                    .Add(Constants.Keys.RangeMid, "darkgray")
                    .Add(Constants.Keys.ProcessHeader, "darkgray");
            default:
                throw new InvalidOperationException();
        }
    }
}