using System.Diagnostics;
using Task.Manager.Cli.Utils;
using Task.Manager.System;
using Task.Manager.System.Controls;

namespace Task.Manager.Gui.Controls;

public sealed class MetreControl : Control
{
    private const int MetreMargin = 2;
    private const char BlockChar = ' ';

    private char[] UnitChars = [' ', '|', '⣿'];
    
    public MetreControl(ISystemTerminal terminal) : base(terminal) { }

    public ConsoleColor ColourSeries1 { get; set; } = ConsoleColor.DarkGray;
    
    public ConsoleColor ColourSeries2 { get; set; } = ConsoleColor.DarkGray;

    private int DrawMetre(
        string label,
        double percentage,
        ConsoleColor colour,
        int offsetX,
        bool isFinalStackSegment = true)
    {
        using TerminalColourRestorer _ = new();

        if (percentage > 1.0) {
            percentage = 1.0;
        }
        
        int units = (int)(percentage * (double)MetreWidth);
        int labelSegmentWidth = label.Length;

        if (units == 0 && percentage > 0) {
            units = 1;
        }

        int segmentWidth = MetreWidth - offsetX;

        // The text in the metre is right-aligned.
        if (label.Length > segmentWidth) {
            label = label.Substring(0, segmentWidth);
            labelSegmentWidth = label.Length;
        }

        ConsoleColor unitFg = MetreStyle == MetreControlStyle.Blocks ? ForegroundColour : colour;
        ConsoleColor unitBg = MetreStyle == MetreControlStyle.Blocks ? colour : BackgroundColour;
        
        if (units + labelSegmentWidth > segmentWidth) {
            // The metre colours will eat into the string. Colour code the string accordingly.
            int numChars = segmentWidth - units;
            int colourStrLen = labelSegmentWidth - numChars;

            label = label.Substring(0, colourStrLen).ToColour(unitFg, unitBg) +
                    label.Substring(colourStrLen, label.Length - colourStrLen)
                         .ToColour(ForegroundColour, BackgroundColour);
            
            units -= colourStrLen;
        }
        else {
            label = label.ToColour(ForegroundColour, BackgroundColour);
        }

        // Filler between the metre and the text (if any).
        int unitFillerLen = Math.Max(0, segmentWidth - units - labelSegmentWidth);
        char unitChar = UnitChars[(int)MetreStyle];

        string unitStr = new string(unitChar, units).ToColour(unitFg, unitBg);
        Terminal.Write(unitStr);

        if (!isFinalStackSegment) {
            return units;
        }
        
        if (unitFillerLen > 0) {
            string unitFillerStr = new string(BlockChar, unitFillerLen).ToColour(ForegroundColour, BackgroundColour);
            Terminal.Write(unitFillerStr);
        }

        if (labelSegmentWidth > 0) {
            Terminal.Write(label);
        }

        return units + unitFillerLen + labelSegmentWidth;
    }
    
    public bool DrawStacked { get; set; } = false;
    
    private void DrawStackedPercentageBar()
    {
        Terminal.Write('[');

        int nchars = 1;

        nchars += DrawMetre(
            LabelSeries1,
            PercentageSeries1,
            ColourSeries1,
            offsetX: 0,
            isFinalStackSegment: false);
        
        _ = DrawMetre(
            LabelSeries2,
            PercentageSeries2,
            ColourSeries2,
            offsetX: nchars - 1,
            isFinalStackSegment: true);
        
        Terminal.Write(']');
    }
    
    private void DrawPercentageBar()
    {
        Terminal.Write('[');

        DrawMetre(
            LabelSeries1,
            PercentageSeries1,
            ColourSeries1,
            offsetX: 0,
            isFinalStackSegment: true);
        
        Terminal.Write(']');
    }

    public string LabelSeries1 { get; set; } = string.Empty;
    
    public string LabelSeries2 { get; set; } = string.Empty;

    public MetreControlStyle MetreStyle { get; set; } = MetreControlStyle.Dots;
    
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