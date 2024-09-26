namespace Task.Manager.Configuration;

public sealed class Constants
{
    public sealed class Sections
    {
        public const string Filter = "filter";
        public const string UX = "ux";
        public const string Stats = "stats";
        public const string Sort = "sort";
        public const string Iterations = "iterations";

        public const string ThemeColour = "theme-colour";
        public const string ThemeMono = "theme-mono";
    }

    public sealed class Keys
    {
        /* General keys. */
        public const string Pid = "pid";
        public const string UserName = "username";
        public const string Process = "process";
        public const string Bars = "bars";
        public const string Cols = "cols";
        public const string Col = "col";
        public const string Asc = "asc";
        public const string Limit = "limit";
        public const string NProcs = "nprocs";
        public const string DefaultTheme = "default-theme";
        
        /* Theme keys. */
        public const string Background = "background";
        public const string BackgroundHighlight = "background-highlight";
        public const string Error = "error";
        public const string Foreground = "foreground";
        public const string ForegroundHighlight = "foreground-highlight";
        public const string Menubar = "menubar";
        public const string RangeHigh = "range-high";
        public const string RangeLow = "range-low";
        public const string RangeMid = "range-mid";
        public const string ProcessHeader = "process-header";
    }
}
