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
        public const string Asc = "asc";
        public const string Col = "col";
        public const string Cols = "cols";
        public const string DefaultTheme = "default-theme";
        public const string Delay = "delay";
        public const string Limit = "limit";
        public const string MetreStyle = "metre-style";
        public const string NProcs = "nprocs";
        public const string Pid = "pid";
        public const string Process = "process";
        public const string UserName = "username";
        
        /* Theme keys. */
        public const string Background = "background";
        public const string BackgroundHighlight = "background-highlight";

        public const string ColCmdNormalUserSpace = "col-cmd-normal-user-space";
        public const string ColCmdLowPriority = "col-cmd-low-priority";
        public const string ColCmdHighCpu = "col-cmd-high-cpu";
        public const string ColCmdIoBound = "col-cmd-io-bound";
        public const string ColCmdScript = "col-cmd-script";
        public const string ColUserCurrentNonRoot  = "col-user-current-non-root";
        public const string ColUserOtherNonRoot  = "col-user-other-non-root";
        public const string ColUserSystem = "col-user-system";
        public const string ColUserRoot = "col-user-root";

        public const string CommandBackground = "command-background";
        public const string CommandForeground = "command-foreground";
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
