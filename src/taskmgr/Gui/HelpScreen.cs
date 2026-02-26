using System.CodeDom;
using System.Text;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public class HelpScreen : Screen
{
    private readonly RunContext runContext;
    private StringBuilder helpText = new();

    public HelpScreen(RunContext runContext) : base(runContext.Terminal) => this.runContext = runContext;

    protected override void OnDraw()
    {
        DrawRectangle(
            X,
            Y,
            Width,
            Height,
            runContext.AppConfig.DefaultTheme.Background);

        Terminal.SetCursorPosition(X, Y);
        Terminal.BackgroundColor = runContext.AppConfig.DefaultTheme.Background;
        Terminal.ForegroundColor = runContext.AppConfig.DefaultTheme.Foreground;
        Terminal.WriteLine(helpText.ToString());
    }

    protected override void OnLoad()
    {
        Terminal.CursorVisible = false;
        
        ConsoleColor fg = runContext.AppConfig.DefaultTheme.Foreground;
        ConsoleColor bg = runContext.AppConfig.DefaultTheme.Background;
        Theme theme = runContext.AppConfig.DefaultTheme;
        
        string version = AssemblyVersionInfo.GetVersion();
        
        helpText.Clear();
        helpText.AppendLine($"taskmgr {version}".ToColour(theme.BackgroundHighlight, bg));
        helpText.AppendLine();
        // Note kernel colours are swapped for contrast and should match what's in HeaderControl.cs
        helpText.AppendLine("Cpu metre:     [".ToColour(fg, bg) +
                            "k low".ToColour(theme.RangeMidBackground, bg) + 
                            " / k mid".ToColour(theme.RangeLowBackground, bg) + 
                            " / k high".ToColour(theme.RangeMidBackground, bg) + 
                            " / u low".ToColour(theme.RangeLowBackground, bg) + 
                            " / u mid".ToColour(theme.RangeMidBackground, bg) + 
                            " / u high".ToColour(theme.RangeHighBackground, bg) +
                            "] k = kernel, u = user".ToColour(fg, bg));
        helpText.AppendLine("Memory metre:  [".ToColour(fg, bg) +
                            "low".ToColour(theme.RangeLowBackground, bg) + 
                            " / mid".ToColour(theme.RangeMidBackground, bg) + 
                            " / high".ToColour(theme.RangeHighBackground, bg) + 
                            " used / total]".ToColour(fg, bg));
#if __WIN32__        
        helpText.AppendLine("Virtual metre: [".ToColour(fg, bg) +
#endif
#if __APPLE__        
        helpText.AppendLine("Swap metre:    [".ToColour(fg, bg) +
#endif
                            "low".ToColour(theme.RangeLowBackground, bg) + 
                            " / mid".ToColour(theme.RangeMidBackground, bg) + 
                            " / high".ToColour(theme.RangeHighBackground, bg) + 
#if __WIN32__        
                            " used / total] Commit Memory".ToColour(fg, bg));
#endif
#if __APPLE__        
                            " used / total] Page File".ToColour(fg, bg));
#endif
        helpText.AppendLine("Disk metre:    [".ToColour(fg, bg) +
                                                "low".ToColour(theme.RangeLowBackground, bg) + 
                                                " / mid".ToColour(theme.RangeMidBackground, bg) + 
                                                " / high".ToColour(theme.RangeHighBackground, bg) + 
                                                " ] Mbps".ToColour(fg, bg));
        helpText.AppendLine("Gpu metre:     [".ToColour(fg, bg) +
                            "low".ToColour(theme.RangeLowBackground, bg) + 
                            " / mid".ToColour(theme.RangeMidBackground, bg) + 
                            " / high".ToColour(theme.RangeHighBackground, bg) + 
                            " ]".ToColour(fg, bg));
        helpText.AppendLine("Gpu Mem metre: [".ToColour(fg, bg) +
                            "low".ToColour(theme.RangeLowBackground, bg) + 
                            " / mid".ToColour(theme.RangeMidBackground, bg) + 
                            " / high".ToColour(theme.RangeHighBackground, bg) + 
                            " used / total] Dedicated Memory".ToColour(fg, bg));
        
        helpText.AppendLine();
        helpText.AppendLine("Process and Path Colours".ToColour(fg, bg));
        helpText.AppendLine("Normal process".ToColour(theme.ColumnCommandNormalUserSpace, bg));
        helpText.AppendLine("Low priority (nice) process".ToColour(theme.ColumnCommandLowPriority, bg));
        helpText.AppendLine("High Cpu usage (> 1 core)".ToColour(theme.ColumnCommandHighCpu, bg));
        helpText.AppendLine("I/O bound process".ToColour(theme.ColumnCommandIoBound, bg));
        helpText.AppendLine();
        helpText.AppendLine("Screen Navigation".ToColour(fg, bg));
        helpText.AppendLine("\u2190    Tab left to next screen component".ToColour(fg, bg));
        helpText.AppendLine("\u2192    Tab right to next screen component".ToColour(fg, bg));
        helpText.AppendLine("\u21B5    Select screen or dialog component".ToColour(fg, bg));
        helpText.AppendLine("ESC  Exit current screen or dialog".ToColour(fg, bg));
        helpText.AppendLine();
        helpText.AppendLine("List Navigation".ToColour(fg, bg));
        helpText.AppendLine("\u2191    Arrow to scroll up".ToColour(fg, bg));
        helpText.AppendLine("\u2193    Arrow to scroll down".ToColour(fg, bg));
        helpText.AppendLine("\u21B5    Enter to select item in list".ToColour(fg, bg));
        helpText.AppendLine("\u2423    Space-bar to check/uncheck item in list".ToColour(fg, bg));
        helpText.AppendLine();
        
        helpText.AppendLine(
@"Function Keys
  F1   Show this help screen
  F2   Show setup screen
  F3   Prompt to sort process list by Process, Pid, User, Pri, Cpu%, Threads, Gpu%, Memory, Disk or Path
  F4   Filter the current process list. If --pid, --username or --process used on start, filter is applied to existing filters
  F5   Show detailed process info, including threads, cpu time, loaded modules and handles
  F6   Terminate selected task in the process list
  F7   Show About dialog
  F10  Exit App

Press ESC to exit Help".ToColour(fg, bg));
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Terminal.CursorVisible = true;
    }
}
