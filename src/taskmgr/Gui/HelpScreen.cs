using System.CodeDom;
using System.Text;
using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Screens;

namespace Task.Manager.Gui;

public class HelpScreen : Screen
{
    private readonly Theme theme;
    private StringBuilder helpText = new();
    
    public HelpScreen(SystemTerminal terminal, Theme theme) : base(terminal) => this.theme = theme;

    protected override void OnDraw()
    {
        DrawRectangle(
            X,
            Y,
            Width,
            Height,
            theme.Background);

        Terminal.SetCursorPosition(X, Y);
        Terminal.BackgroundColor = theme.Background;
        Terminal.ForegroundColor = theme.Foreground;
        Terminal.WriteLine(helpText.ToString());
    }

    protected override void OnLoad()
    {
        string version = "1.0.0";
        
        helpText.Clear();
        helpText.AppendLine($"taskmgr {version}".ToColour(theme.BackgroundHighlight, theme.Background));
        helpText.AppendLine("Released under the <whatever> license".ToColour(theme.BackgroundHighlight, theme.Background));
        helpText.AppendLine();
        // Note kernel colours are swapped for contrast and should match what's in HeaderControl.cs
        helpText.AppendLine("CPU metre:     [".ToColour(theme.Foreground, theme.Background) +
                            "k low".ToColour(theme.RangeMidBackground, theme.Background) + 
                            " / k mid".ToColour(theme.RangeLowBackground, theme.Background) + 
                            " / k high".ToColour(theme.RangeMidBackground, theme.Background) + 
                            " / u low".ToColour(theme.RangeLowBackground, theme.Background) + 
                            " / u mid".ToColour(theme.RangeMidBackground, theme.Background) + 
                            " / u high".ToColour(theme.RangeHighBackground, theme.Background) +
                            "] k = kernel, u = user".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("Memory metre:  [".ToColour(theme.Foreground, theme.Background) +
                            "low".ToColour(theme.RangeLowBackground, theme.Background) + 
                            " / mid".ToColour(theme.RangeMidBackground, theme.Background) + 
                            " / high".ToColour(theme.RangeHighBackground, theme.Background) + 
                            " used / total]".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("Virtual metre: [".ToColour(theme.Foreground, theme.Background) +
                            "low".ToColour(theme.RangeLowBackground, theme.Background) + 
                            " / mid".ToColour(theme.RangeMidBackground, theme.Background) + 
                            " / high".ToColour(theme.RangeHighBackground, theme.Background) + 
                            " used / total] Page File".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("Disk metre:    [".ToColour(theme.Foreground, theme.Background) +
                            "low".ToColour(theme.RangeLowBackground, theme.Background) + 
                            " / mid".ToColour(theme.RangeMidBackground, theme.Background) + 
                            " / high".ToColour(theme.RangeHighBackground, theme.Background) + 
                            " ] Mbps".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine();
        helpText.AppendLine("Screen Navigation".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("\u2190    Tab left to next screen component".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("\u2192    Tab right to next screen component".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("\u21B5    Select screen or dialog component".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("ESC  Exit current screen or dialog".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine();
        helpText.AppendLine("List Navigation".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("\u2191    Scroll up".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("\u2193    Scroll down".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("\u21B5    Select item in List".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine();
        helpText.AppendLine("Function Keys".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("F1   Show this help screen".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("F2   Show setup screen".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("F3   Prompt to sort process list by Process, Pid, User, Pri, Cpu%, Threads, Memory, Disk or Path".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("F4   Filter the current process list. If --pid, --username or --process used on start, filter is applied to existing filters".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("F5   Show detailed process info, including threads, cpu time, loaded modules and handles".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("F6   Terminate selected task in the process list".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine("F7   Show About dialog".ToColour(theme.Foreground, theme.Background));
        helpText.AppendLine();
        helpText.AppendLine("Press ESC to exit Help".ToColour(theme.Foreground, theme.Background));
    }
}
