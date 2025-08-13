using Task.Manager.Cli.Utils;
using Task.Manager.System;
using Task.Manager.System.Controls;

namespace Task.Manager.Gui.Controls;

public sealed class MetreControl : Control
{
    private const char MetreLeftBracket = '[';
    private const char MetreRightBracket = ']';
    private const int MetreMargin = 2;
    
    public MetreControl(ISystemTerminal terminal) : base(terminal) { }

    public ConsoleColor ColourSeries1 { get; set; } = ConsoleColor.DarkGray;
    
    public ConsoleColor ColourSeries2 { get; set; } = ConsoleColor.DarkGray;

    private int DrawMeter(
        string label,
        double percentage,
        ConsoleColor colour,
        int offsetX,
        bool drawBackground = true)
    {
        using TerminalColourRestorer _ = new();
        
        int nchars = 0;
        int inverseBars = (int)(percentage * (double)MetreWidth);

        if (inverseBars == 0 && percentage > 0) {
            inverseBars = 1;
        }

        Terminal.BackgroundColor = colour;
        Terminal.ForegroundColor = ForegroundColour;

        if (inverseBars > 0) {
            Terminal.Write(label);
            nchars += label.Length;
        }

        for (int i = nchars; i < inverseBars; i++) {
            Terminal.Write(' ');
            nchars++;
        }

        if (drawBackground)
        {
            Terminal.BackgroundColor = BackgroundColour;

            for (int i = 0; i < MetreWidth - (inverseBars + offsetX); i++) {
                Terminal.Write(' ');
                nchars++;
            }
        }
        
        return nchars;
    }

    public bool DrawStacked { get; set; } = false;
    
    private void DrawStackedPercentageBar()
    {
        Terminal.Write('[');

        int nchars = 1;

        nchars += DrawMeter(
            LabelSeries1,
            PercentageSeries1,
            ColourSeries1,
            offsetX: 0,
            drawBackground: false);
        
        _ = DrawMeter(
            LabelSeries2,
            PercentageSeries2,
            ColourSeries2,
            offsetX: nchars - 1,
            drawBackground: true);
        
        Terminal.Write(']');
    }
    
    private void DrawPercentageBar()
    {
        Terminal.Write('[');

        DrawMeter(
            LabelSeries1,
            PercentageSeries1,
            ColourSeries1,
            offsetX: 0,
            drawBackground: true);
        
        Terminal.Write(']');
    }

    public string LabelSeries1 { get; set; } = string.Empty;
    
    public string LabelSeries2 { get; set; } = string.Empty;

    private int MetreWidth => Width - Text.Length - MetreMargin;

    protected override void OnDraw()
    {
        Terminal.SetCursorPosition(X, Y);
        Terminal.BackgroundColor = BackgroundColour;
        Terminal.ForegroundColor = ForegroundColour;
        Terminal.Write(Text);
        Terminal.Write(' ');
        
        if (!DrawStacked) {
            DrawPercentageBar();
        }
        else {
            DrawStackedPercentageBar();
        }
    }
    
    public double PercentageSeries1 { get; set; }
    
    public double PercentageSeries2 { get; set; }

    public string Text { get; set; } = string.Empty;
}