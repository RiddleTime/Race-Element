using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.Overlay.Util;

public sealed class InfoPanel
{
    private readonly Font _font;
    private readonly int MaxWidth;
    public int X = 0;
    public int Y = 0;

    public int FontHeight { get; private set; }

    public bool DrawBackground = true;
    public bool DrawValueBackground = true;
    public bool DrawRowLines { get; set; } = true;
    public int FirstRowLine { get; set; } = 0;
    public int ExtraLineSpacing { get; set; } = 0;


    private bool MaxTitleWidthSet = false;
    public float MaxTitleWidth { get; private set; } = 0;

    private readonly int _addMonoY = 0;

    private CachedBitmap _cachedBackground;
    private CachedBitmap _cachedLine;

    private int previousLineCount = 0;

    private readonly object _lockObj = new();

    public InfoPanel(double fontSize, int maxWidth)
    {
        fontSize.ClipMin(10);
        this.MaxWidth = maxWidth;
        this._font = FontUtil.FontSegoeMono((float)fontSize);
        this.FontHeight = _font.Height;
        _addMonoY = _font.Height / 8;
    }

    private readonly List<IPanelLine> Lines = [];

    public void SetBackground()
    {
        _cachedBackground = new CachedBitmap(MaxWidth, Lines.Count * (this.FontHeight + ExtraLineSpacing), g =>
        {
            using SolidBrush backgroundBrush = new(Color.FromArgb(158, 0, 0, 0));
            g.FillRoundedRectangle(backgroundBrush, new Rectangle(X, Y, this.MaxWidth, Lines.Count * (this.FontHeight + ExtraLineSpacing)), 4);

            if (DrawValueBackground)
            {
                int y = Y + _font.Height * FirstRowLine;
                int height = (Lines.Count - FirstRowLine) * (this.FontHeight + ExtraLineSpacing) + (int)_addMonoY - 2;

                int characterWidth = (int)g.MeasureString("M", _font).Width / 2;
                int x = (int)(MaxTitleWidth + characterWidth);
                int width = (int)(MaxWidth - MaxTitleWidth - characterWidth);
                using SolidBrush valueBackgroundBrush = new(Color.FromArgb(80, Color.Black));
                g.FillRoundedRectangle(valueBackgroundBrush, new Rectangle(x, y, width, height), 4);
            }
        });
    }

    private readonly StringFormat _rightAlligned = new() { Alignment = StringAlignment.Center };

    public void Draw(Graphics g)
    {
        if (!MaxTitleWidthSet)
        {
            UpdateMaxTitleWidth(g);
            MaxTitleWidthSet = true;
        }

        if (DrawBackground)
        {
            if (Lines.Count != previousLineCount)
            {
                SetBackground();
                previousLineCount = Lines.Count;
            }

            _cachedBackground?.Draw(g, new Point(X, Y));
        }

        TextRenderingHint previousHint = g.TextRenderingHint;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.TextContrast = 1;

        lock (_lockObj)
        {
            ReadOnlySpan<IPanelLine> lines = CollectionsMarshal.AsSpan(Lines);

            int length = Lines.Count;
            int counter = 0;

            while (counter < length)
            {
                if (DrawRowLines && counter > FirstRowLine)
                {
                    float rowY = counter * (this.FontHeight + ExtraLineSpacing);

                    _cachedLine ??= new CachedBitmap(this.MaxWidth - 2, 1, cr =>
                        {
                            using Pen linePen = new(Color.FromArgb(42, Color.White));
                            cr.DrawLine(linePen, new Point(1, 0), new Point(this.MaxWidth - 2, 0));
                        });

                    _cachedLine.Draw(g, new Point(X + 1, (int)rowY));
                }

                IPanelLine line = lines[counter];

                if (line is TextLine)
                {
                    TextLine textLine = (TextLine)line;

                    g.DrawStringWithShadow($"{textLine.Title}", _font, Color.White, new PointF(X, Y + counter * (this.FontHeight + ExtraLineSpacing) + _addMonoY));
                    Rectangle valueRectangle = new((int)(X + MaxTitleWidth),
                            Y + counter * (this.FontHeight + ExtraLineSpacing) + _addMonoY,
                            (int)(MaxWidth - MaxTitleWidth),
                            this.FontHeight);
                    g.DrawStringWithShadow($"{textLine.Value}", _font, textLine.ValueBrush, valueRectangle, _rightAlligned);
                }

                if (line is TitledProgressBarLine)
                {
                    TitledProgressBarLine bar = (TitledProgressBarLine)line;
                    g.DrawStringWithShadow($"{bar.Title}", _font, Brushes.White, new PointF(X, Y + counter * FontHeight));

                    ProgressBar progressBar = new(bar.Min, bar.Max, bar.Value);
                    progressBar.Draw(g, bar.BarColor, Brushes.Transparent, new Rectangle((int)(X + MaxTitleWidth + _font.Size), Y + counter * (this.FontHeight + ExtraLineSpacing) + 1, (int)(MaxWidth - MaxTitleWidth - _font.Size) - 4, (int)FontHeight - 2), false, false);

                    string percent = $"{(bar.Max / bar.Value * 100):F1}%";
                    SizeF textWidth = g.MeasureString(percent, _font);
                    g.DrawStringWithShadow($"{percent}", _font, Brushes.White, new PointF((int)(X + (MaxWidth - MaxTitleWidth)) - textWidth.Width / 2, Y + counter * (this.FontHeight + ExtraLineSpacing) + _addMonoY));
                }

                if (line is CenterTextedProgressBarLine)
                {
                    CenterTextedProgressBarLine bar = (CenterTextedProgressBarLine)line;

                    ProgressBar progressBar = new(bar.Min, bar.Max, bar.Value);
                    progressBar.Draw(g, bar.BarColor, Brushes.Transparent, new Rectangle(X + 3, Y + counter * FontHeight + 1, (int)MaxWidth - 6, (int)FontHeight - 2), false, false);

                    SizeF textWidth = g.MeasureString(bar.CenteredText, _font);
                    g.DrawStringWithShadow($"{bar.CenteredText}", _font, Color.White, new PointF(X + MaxWidth / 2 - textWidth.Width / 2, Y + counter * (this.FontHeight + ExtraLineSpacing) + _addMonoY), 1f);
                }

                if (line is CenteredTextedDeltabarLine)
                {
                    CenteredTextedDeltabarLine bar = (CenteredTextedDeltabarLine)line;

                    DeltaBar deltaBar = new(bar.Min, bar.Max, bar.Value);
                    deltaBar.Draw(g, X + 1, Y + counter * FontHeight + 1, (int)MaxWidth - 2, (int)FontHeight - 2);

                    SizeF textWidth = g.MeasureString(bar.CenteredText, _font);
                    g.DrawStringWithShadow($"{bar.CenteredText}", _font, Brushes.White, new PointF(X + MaxWidth / 2 - textWidth.Width / 2, Y + counter * (this.FontHeight + ExtraLineSpacing) + _addMonoY));
                }


                counter++;
                length = Lines.Count;
            }
        }
        g.TextRenderingHint = previousHint;

        Lines.Clear();
    }

    private void UpdateMaxTitleWidth(Graphics g)
    {
        lock (_lockObj)
        {
            int length = Lines.Count;
            int counter = 0;
            while (counter < length)
            {
                IPanelLine line = Lines[counter];

                if (line is TextLine)
                {
                    TextLine textLine = (TextLine)line;
                    SizeF titleWidth;
                    if ((titleWidth = g.MeasureString(textLine.Title, _font)).Width > MaxTitleWidth)
                        MaxTitleWidth = titleWidth.Width;
                }

                if (line is TitledProgressBarLine)
                {
                    TitledProgressBarLine titledProgressBar = (TitledProgressBarLine)line;
                    SizeF titleWidth = g.MeasureString(titledProgressBar.Title, _font);
                    if (titleWidth.Width > MaxTitleWidth)
                        MaxTitleWidth = titleWidth.Width;
                }

                counter++;
                length = Lines.Count;
            }
        }
    }


    public void AddLine(string title, string value, bool valueIsMonoFont = true)
    {
        Lines.Add(new TextLine() { Title = title, Value = value });
    }

    public void AddLine(string title, string value, Brush valueBrush)
    {
        Lines.Add(new TextLine() { Title = title, Value = value, ValueBrush = valueBrush });
    }

    public void AddProgressBar(string title, double min, double max, double value)
    {
        Lines.Add(new TitledProgressBarLine() { Title = title, Min = min, Max = max, Value = value });
    }

    public void AddProgressBar(string title, double min, double max, double value, Brush barColor)
    {
        Lines.Add(new TitledProgressBarLine() { Title = title, Min = min, Max = max, Value = value, BarColor = barColor });
    }

    public void AddProgressBarWithCenteredText(string centeredText, double min, double max, double value)
    {
        Lines.Add(new CenterTextedProgressBarLine() { CenteredText = centeredText, Min = min, Max = max, Value = value });
    }

    public void AddProgressBarWithCenteredText(string centeredText, double min, double max, double value, Brush barColor)
    {
        Lines.Add(new CenterTextedProgressBarLine() { CenteredText = centeredText, Min = min, Max = max, Value = value, BarColor = barColor });
    }

    public void AddDeltaBarWithCenteredText(string centeredText, double min, double max, double value)
    {
        Lines.Add(new CenteredTextedDeltabarLine() { CenteredText = centeredText, Min = min, Max = max, Value = value });
    }

    private sealed class TextLine : IPanelLine
    {
        internal string Title { get; set; }
        internal string Value { get; set; }
        internal Brush ValueBrush { get; set; } = Brushes.White;
    }

    private sealed class TitledProgressBarLine : IPanelLine
    {
        internal string Title { get; set; }
        internal double Min { get; set; }
        internal double Max { get; set; }
        internal double Value { get; set; }
        internal Brush BarColor { get; set; } = Brushes.OrangeRed;
    }

    private sealed class CenterTextedProgressBarLine : IPanelLine
    {
        internal string CenteredText { get; set; }
        internal double Min { get; set; }
        internal double Max { get; set; }
        internal double Value { get; set; }
        internal Brush BarColor { get; set; } = Brushes.OrangeRed;
    }

    private sealed class CenteredTextedDeltabarLine : IPanelLine
    {
        internal string CenteredText { get; set; }
        internal double Min { get; set; }
        internal double Max { get; set; }
        internal double Value { get; set; }
    }

    private interface IPanelLine { }
}
