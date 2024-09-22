using Task.Manager.System.Configuration;

namespace Task.Manager.Configuration;

public static class ConfigBuilder
{
    public static Config BuildDefault()
    {
        var config = new Config();

        config.Sections.Add(new ConfigSection(Constants.Sections.Filter)
            .Add(Constants.Keys.Pid, "-1")
            .Add(Constants.Keys.UserName, "")
            .Add(Constants.Keys.Process, ""));

        /*
         * Setting bars = true will draw "|" pipes in all metres in the interface.
         * Setting to false will fill the char cell with the nominated colour.
         */
        config.Sections.Add(new ConfigSection(Constants.Sections.Metres)
            .Add(Constants.Keys.Bars, "false"));

        config.Sections.Add(new ConfigSection(Constants.Sections.Stats)
            /* These cols must match the Configuration.Statistics enum members. */
            .Add(Constants.Keys.Cols, "pid, process, user, pri, cpu, mem, virt, thrd, disk")
            .Add(Constants.Keys.NProcs, "-1"));
        
        config.Sections.Add(new ConfigSection(Constants.Sections.Sort)
            .Add(Constants.Keys.Col, "pid"));
        
        /* Max number of iterations to execute before exiting. <= 0 infinite. */
        config.Sections.Add(new ConfigSection(Constants.Sections.Iterations)
            .Add(Constants.Keys.Limit, "0"));
        
        config.Sections.Add(new ConfigSection(Constants.Sections.ThemeColour)
            .Add(Constants.Keys.Background, "black")
            .Add(Constants.Keys.BackgroundHighlight, "black")
            .Add(Constants.Keys.Error, "red")
            .Add(Constants.Keys.Foreground, "darkgray")
            .Add(Constants.Keys.ForegroundHighlight, "white")
            .Add(Constants.Keys.Menubar, "gray")
            .Add(Constants.Keys.RangeHigh, "red")
            .Add(Constants.Keys.RangeLow, "green")
            .Add(Constants.Keys.RangeMid, "yellow")
            .Add(Constants.Keys.ProcessHeader, "cyan"));
        
        config.Sections.Add(new ConfigSection(Constants.Sections.ThemeMono)
            .Add(Constants.Keys.Background, "black")
            .Add(Constants.Keys.BackgroundHighlight, "black")
            .Add(Constants.Keys.Error, "gray")
            .Add(Constants.Keys.Foreground, "darkgray")
            .Add(Constants.Keys.ForegroundHighlight, "white")
            .Add(Constants.Keys.Menubar, "gray")
            .Add(Constants.Keys.RangeHigh, "gray")
            .Add(Constants.Keys.RangeLow, "white")
            .Add(Constants.Keys.RangeMid, "darkgray")
            .Add(Constants.Keys.ProcessHeader, "darkgray"));

        return config;
    }
}