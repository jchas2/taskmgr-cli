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
        string version = "1.0.0";
        
        helpText.Clear();
        helpText.AppendLine($"taskmgr {version}".ToColour(runContext.AppConfig.DefaultTheme.BackgroundHighlight, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("Released under the <whatever> license".ToColour(runContext.AppConfig.DefaultTheme.BackgroundHighlight, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine();
        // Note kernel colours are swapped for contrast and should match what's in HeaderControl.cs
        helpText.AppendLine("Cpu metre:     [".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background) +
                            "k low".ToColour(runContext.AppConfig.DefaultTheme.RangeMidBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " / k mid".ToColour(runContext.AppConfig.DefaultTheme.RangeLowBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " / k high".ToColour(runContext.AppConfig.DefaultTheme.RangeMidBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " / u low".ToColour(runContext.AppConfig.DefaultTheme.RangeLowBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " / u mid".ToColour(runContext.AppConfig.DefaultTheme.RangeMidBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " / u high".ToColour(runContext.AppConfig.DefaultTheme.RangeHighBackground, runContext.AppConfig.DefaultTheme.Background) +
                            "] k = kernel, u = user".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("Memory metre:  [".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background) +
                            "low".ToColour(runContext.AppConfig.DefaultTheme.RangeLowBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " / mid".ToColour(runContext.AppConfig.DefaultTheme.RangeMidBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " / high".ToColour(runContext.AppConfig.DefaultTheme.RangeHighBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " used / total]".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
#if __WIN32__        
        helpText.AppendLine("Virtual metre: [".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background) +
#endif
#if __APPLE__        
        helpText.AppendLine("Swap metre:    [".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background) +
#endif
                            "low".ToColour(runContext.AppConfig.DefaultTheme.RangeLowBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " / mid".ToColour(runContext.AppConfig.DefaultTheme.RangeMidBackground, runContext.AppConfig.DefaultTheme.Background) + 
                            " / high".ToColour(runContext.AppConfig.DefaultTheme.RangeHighBackground, runContext.AppConfig.DefaultTheme.Background) + 
#if __WIN32__        
                            " used / total] Commit Memory".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
#endif
#if __APPLE__        
                            " used / total] Page File".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
#endif
        helpText.AppendLine("Disk metre:    [".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background) +
                                                "low".ToColour(runContext.AppConfig.DefaultTheme.RangeLowBackground, runContext.AppConfig.DefaultTheme.Background) + 
                                                " / mid".ToColour(runContext.AppConfig.DefaultTheme.RangeMidBackground, runContext.AppConfig.DefaultTheme.Background) + 
                                                " / high".ToColour(runContext.AppConfig.DefaultTheme.RangeHighBackground, runContext.AppConfig.DefaultTheme.Background) + 
                                                " ] Mbps".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine();
        helpText.AppendLine("Process and Path Colours".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("Normal process".ToColour(runContext.AppConfig.DefaultTheme.ColumnCommandNormalUserSpace, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("Low priority (nice) process".ToColour(runContext.AppConfig.DefaultTheme.ColumnCommandLowPriority, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("High Cpu usage (> 1 core)".ToColour(runContext.AppConfig.DefaultTheme.ColumnCommandHighCpu, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("I/O bound process".ToColour(runContext.AppConfig.DefaultTheme.ColumnCommandIoBound, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine();
        helpText.AppendLine("Screen Navigation".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("\u2190    Tab left to next screen component".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("\u2192    Tab right to next screen component".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("\u21B5    Select screen or dialog component".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("ESC  Exit current screen or dialog".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine();
        helpText.AppendLine("List Navigation".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("\u2191    Arrow to scroll up".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("\u2193    Arrow to scroll down".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("\u21B5    Enter to select item in list".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine("\u2423    Space-bar to check/uncheck item in list".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
        helpText.AppendLine();
        
        helpText.AppendLine(
@"Function Keys
  F1   Show this help screen
  F2   Show setup screen
  F3   Prompt to sort process list by Process, Pid, User, Pri, Cpu%, Threads, Memory, Disk or Path
  F4   Filter the current process list. If --pid, --username or --process used on start, filter is applied to existing filters
  F5   Show detailed process info, including threads, cpu time, loaded modules and handles
  F6   Terminate selected task in the process list
  F7   Show About dialog

  Press ESC to exit Help".ToColour(runContext.AppConfig.DefaultTheme.Foreground, runContext.AppConfig.DefaultTheme.Background));
    }
}
