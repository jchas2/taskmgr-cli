using Task.Manager.System.Configuration;

namespace Task.Manager.Configuration;

public static class ConfigBuilder
{
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

    public static ConfigSection BuildSection(string name)
    {
        name = name.ToLower();
        switch (name) {
            case Constants.Sections.Filter:
                return new ConfigSection(Constants.Sections.Filter)
                    .Add(Constants.Keys.Pid, "-1")
                    .Add(Constants.Keys.UserName, "")
                    .Add(Constants.Keys.Process, "");
            case Constants.Sections.UX:
                return new ConfigSection(Constants.Sections.UX)
                    .Add(Constants.Keys.Bars, "false")
                    .Add(Constants.Keys.DefaultTheme, Constants.Sections.ThemeColour);
            case Constants.Sections.Stats:
                return new ConfigSection(Constants.Sections.Stats)
                    /* These cols must match the Configuration.Statistics enum members. */
                    .Add(Constants.Keys.Cols, "pid, process, user, pri, cpu, mem, virt, thrd, disk")
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