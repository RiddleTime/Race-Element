using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ACC_Manager.Util.NumberExtensions;
using ACCManager.HUD.Overlay.OverlayUtil;


namespace ACCManager.HUD.Overlay.Util
{
    public class InfoPanel
    {
        private const float ShadowDistance = 0.75f;
        private readonly Brush ShadowBrush = new SolidBrush(Color.FromArgb(60, Color.Black));

        private readonly Font Font;
        private readonly Font MonoFont;

        private readonly int MaxWidth;
        public int X = 0;
        public int Y = 0;

        private int _fontHeight;
        public int FontHeight { get { return this._fontHeight; } private set { this._fontHeight = value; } }
        public bool DrawBackground = true;


        private bool MaxTitleWidthSet = false;
        public float MaxTitleWidth { get; private set; } = 0;

        private int _yMono = 0;

        public InfoPanel(double fontSize, int maxWidth)
        {
            fontSize.ClipMin(9);
            this.MaxWidth = maxWidth;
            this.Font = FontUtil.FontOrbitron((float)fontSize);
            this.MonoFont = FontUtil.FontUnispace((float)fontSize);
            this.FontHeight = Font.Height;
            _yMono = MonoFont.Height / 6;
        }

        private List<IPanelLine> Lines = new List<IPanelLine>();

        public void Draw(Graphics g)
        {
            if (!MaxTitleWidthSet)
            {
                UpdateMaxTitleWidth(g);
                MaxTitleWidthSet = true;
            }

            if (DrawBackground)
            {
                SmoothingMode previous = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(140, 0, 0, 0)), new Rectangle(X, Y, this.MaxWidth, Lines.Count * this.FontHeight), 4);
                g.SmoothingMode = previous;
            }

            TextRenderingHint previousHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.TextContrast = 1;

            lock (Lines)
            {
                int length = Lines.Count;
                int counter = 0;
                while (counter < length)
                {
                    IPanelLine line = Lines[counter];

                    if (line.GetType() == typeof(TextLine))
                    {
                        TextLine textLine = (TextLine)line;
                        g.DrawString($"{textLine.Title}", Font, ShadowBrush, new PointF(X + ShadowDistance, Y + counter * FontHeight + ShadowDistance));
                        g.DrawString($"{textLine.Title}", Font, Brushes.White, new PointF(X, Y + counter * FontHeight));

                        int yAdd = textLine.ValueFontIsMono ? _yMono : 0;
                        g.DrawString($"{textLine.Value}", textLine.ValueFontIsMono ? MonoFont : Font, ShadowBrush, new PointF(X + MaxTitleWidth + Font.Size + ShadowDistance, Y + counter * FontHeight + yAdd + ShadowDistance));
                        g.DrawString($"{textLine.Value}", textLine.ValueFontIsMono ? MonoFont : Font, textLine.ValueBrush, new PointF(X + MaxTitleWidth + Font.Size, Y + counter * FontHeight + yAdd));
                    }

                    if (line.GetType() == typeof(TitledProgressBarLine))
                    {
                        TitledProgressBarLine bar = (TitledProgressBarLine)line;
                        g.DrawString($"{bar.Title}", Font, ShadowBrush, new PointF(X + ShadowDistance, Y + counter * FontHeight + ShadowDistance));
                        g.DrawString($"{bar.Title}", Font, Brushes.White, new PointF(X, Y + counter * FontHeight));

                        ProgressBar progressBar = new ProgressBar(bar.Min, bar.Max, bar.Value);
                        progressBar.Draw(g, (int)(X + MaxTitleWidth + Font.Size), Y + counter * FontHeight + 1, (int)(MaxWidth - MaxTitleWidth - Font.Size) - 4, (int)FontHeight - 2, bar.BarColor);

                        string percent = $"{(bar.Max / bar.Value * 100):F1}%";
                        SizeF textWidth = g.MeasureString(percent, bar.ValueFontIsMono ? MonoFont : Font);
                        int yMono = bar.ValueFontIsMono ? _yMono : 0;
                        g.DrawString($"{percent}", bar.ValueFontIsMono ? MonoFont : Font, Brushes.White, new PointF((int)(X + (MaxWidth - MaxTitleWidth)) - textWidth.Width / 2, Y + counter * FontHeight + yMono));
                    }

                    if (line.GetType() == typeof(CenterTextedProgressBarLine))
                    {
                        CenterTextedProgressBarLine bar = (CenterTextedProgressBarLine)line;

                        ProgressBar progressBar = new ProgressBar(bar.Min, bar.Max, bar.Value);
                        progressBar.Draw(g, X + 3, Y + counter * FontHeight + 1, (int)MaxWidth - 6, (int)FontHeight - 2, bar.BarColor);

                        SizeF textWidth = g.MeasureString(bar.CenteredText, bar.ValueFontIsMono ? MonoFont : Font);
                        int yMono = bar.ValueFontIsMono ? _yMono : 0;
                        g.DrawString($"{bar.CenteredText}", bar.ValueFontIsMono ? MonoFont : Font, ShadowBrush, new PointF(X + MaxWidth / 2 - textWidth.Width / 2 + ShadowDistance, Y + counter * FontHeight + yMono + ShadowDistance));
                        g.DrawString($"{bar.CenteredText}", bar.ValueFontIsMono ? MonoFont : Font, Brushes.White, new PointF(X + MaxWidth / 2 - textWidth.Width / 2, Y + counter * FontHeight + yMono));

                    }

                    if (line.GetType() == typeof(CenteredTextedDeltabarLine))
                    {
                        CenteredTextedDeltabarLine bar = (CenteredTextedDeltabarLine)line;

                        DeltaBar deltaBar = new DeltaBar(bar.Min, bar.Max, bar.Value);
                        deltaBar.Draw(g, X + 1, Y + counter * FontHeight + 1, (int)MaxWidth - 2, (int)FontHeight - 2);

                        SizeF textWidth = g.MeasureString(bar.CenteredText, bar.ValueFontIsMono ? MonoFont : Font);
                        int yMono = bar.ValueFontIsMono ? _yMono : 0;
                        g.DrawString($"{bar.CenteredText}", bar.ValueFontIsMono ? MonoFont : Font, ShadowBrush, new PointF(X + MaxWidth / 2 - textWidth.Width / 2 + ShadowDistance, Y + counter * FontHeight + yMono + ShadowDistance));
                        g.DrawString($"{bar.CenteredText}", bar.ValueFontIsMono ? MonoFont : Font, Brushes.White, new PointF(X + MaxWidth / 2 - textWidth.Width / 2, Y + counter * FontHeight + yMono));
                    }


                    counter++;
                    length = Lines.Count;
                }
            }

            g.TextRenderingHint = previousHint;

            lock (Lines)
                Lines.Clear();
        }

        private void UpdateMaxTitleWidth(Graphics g)
        {
            lock (Lines)
            {
                int length = Lines.Count;
                int counter = 0;
                while (counter < length)
                {
                    IPanelLine line = Lines[counter];


                    if (line.GetType() == typeof(TextLine))
                    {
                        TextLine textLine = (TextLine)line;
                        SizeF titleWidth;
                        if ((titleWidth = g.MeasureString(textLine.Title, Font)).Width > MaxTitleWidth)
                            MaxTitleWidth = titleWidth.Width;
                    }

                    if (line.GetType() == typeof(TitledProgressBarLine))
                    {
                        TitledProgressBarLine titledProgressBar = (TitledProgressBarLine)line;
                        SizeF titleWidth;
                        if ((titleWidth = g.MeasureString(titledProgressBar.Title, Font)).Width > MaxTitleWidth)
                            MaxTitleWidth = titleWidth.Width;
                    }


                    counter++;
                    length = Lines.Count;
                }
            }
        }


        public void AddLine(string title, string value, bool valueIsMonoFont = true)
        {
            Lines.Add(new TextLine() { Title = title, Value = value, ValueFontIsMono = valueIsMonoFont });
        }

        public void AddLine(string title, string value, Brush valueBrush, bool valueIsMonoFont = true)
        {
            Lines.Add(new TextLine() { Title = title, Value = value, ValueBrush = valueBrush, ValueFontIsMono = valueIsMonoFont });
        }

        public void AddProgressBar(string title, double min, double max, double value)
        {
            Lines.Add(new TitledProgressBarLine() { Title = title, Min = min, Max = max, Value = value });
        }

        public void AddProgressBar(string title, double min, double max, double value, Brush barColor)
        {
            Lines.Add(new TitledProgressBarLine() { Title = title, Min = min, Max = max, Value = value, BarColor = barColor });
        }

        public void AddProgressBarWithCenteredText(string centeredText, double min, double max, double value, bool valueIsMonoFont = true)
        {
            Lines.Add(new CenterTextedProgressBarLine() { CenteredText = centeredText, Min = min, Max = max, Value = value, ValueFontIsMono = valueIsMonoFont });
        }

        public void AddProgressBarWithCenteredText(string centeredText, double min, double max, double value, Brush barColor, bool valueIsMonoFont = true)
        {
            Lines.Add(new CenterTextedProgressBarLine() { CenteredText = centeredText, Min = min, Max = max, Value = value, BarColor = barColor, ValueFontIsMono = valueIsMonoFont });
        }

        public void AddDeltaBarWithCenteredText(string centeredText, double min, double max, double value, bool valueIsMonoFont = true)
        {
            Lines.Add(new CenteredTextedDeltabarLine() { CenteredText = centeredText, Min = min, Max = max, Value = value, ValueFontIsMono = valueIsMonoFont });
        }

        private class TextLine : IPanelLine
        {
            internal string Title { get; set; }
            internal string Value { get; set; }
            internal Brush ValueBrush { get; set; } = Brushes.White;
            internal bool ValueFontIsMono { get; set; }
        }

        private class TitledProgressBarLine : IPanelLine
        {
            internal string Title { get; set; }
            internal double Min { get; set; }
            internal double Max { get; set; }
            internal double Value { get; set; }
            internal Brush BarColor { get; set; } = Brushes.OrangeRed;
            internal bool ValueFontIsMono { get; set; }
        }

        private class CenterTextedProgressBarLine : IPanelLine
        {
            internal string CenteredText { get; set; }
            internal double Min { get; set; }
            internal double Max { get; set; }
            internal double Value { get; set; }
            internal Brush BarColor { get; set; } = Brushes.OrangeRed;
            internal bool ValueFontIsMono { get; set; }
        }

        private class CenteredTextedDeltabarLine : IPanelLine
        {
            internal string CenteredText { get; set; }
            internal double Min { get; set; }
            internal double Max { get; set; }
            internal double Value { get; set; }
            internal bool ValueFontIsMono { get; set; }
        }

        private interface IPanelLine { }
    }
}
