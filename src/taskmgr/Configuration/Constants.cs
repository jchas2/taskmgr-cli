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
        public const string ThemeMsDos = "theme-msdos";
        public const string ThemeTokyoNight = "theme-tokyo-night";
        public const string ThemeMatrix = "theme-matrix";
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
        public const string HeaderForeground = "header-foreground";
        public const string HeaderBackground = "header-background";
        public const string MenubarForeground = "menubar-foreground";
        public const string MenubarBackground = "menubar-background";
        public const string RangeHighBackground = "range-high-background";
        public const string RangeLowBackground = "range-low-background";
        public const string RangeMidBackground = "range-mid-background";
        public const string RangeHighForeground = "range-high-foreground";
        public const string RangeLowForeground = "range-low-foreground";
        public const string RangeMidForeground = "range-mid-foreground";
    }
}
