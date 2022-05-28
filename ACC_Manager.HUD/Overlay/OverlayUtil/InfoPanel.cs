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
using ACC_Manager.Util.NumberExtensions;
using ACCManager.HUD.Overlay.OverlayUtil;


namespace ACCManager.HUD.Overlay.Util
{
    public class InfoPanel
    {
        private const float ShadowDistance = 0.75f;
        private readonly Color _shadowColor = Color.FromArgb(60, Color.Black);
        private readonly Brush ShadowBrush = new SolidBrush(Color.FromArgb(60, Color.Black));

        private readonly Font Font;

        private readonly int MaxWidth;
        public int X = 0;
        public int Y = 0;

        private int _fontHeight;
        public int FontHeight { get { return this._fontHeight; } private set { this._fontHeight = value; } }
        public bool DrawBackground = true;
        public bool DrawRowLines { get; set; } = true;
        public int FirstRowLine { get; set; } = 0;


        private bool MaxTitleWidthSet = false;
        public float MaxTitleWidth { get; private set; } = 0;

        private readonly int _addMonoY = 0;

        public InfoPanel(double fontSize, int maxWidth)
        {
            fontSize.ClipMin(9);
            this.MaxWidth = maxWidth;
            this.Font = FontUtil.FontUnispace((float)fontSize);
            this.FontHeight = Font.Height;
            _addMonoY = Font.Height / 8;
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
                    if (DrawRowLines && counter > FirstRowLine)
                    {
                        float rowY = counter * FontHeight;
                        g.DrawLine(new Pen(Color.FromArgb(45, Color.White)), new Point(X, (int)rowY), new Point(this.MaxWidth, (int)rowY));
                    }

                    IPanelLine line = Lines[counter];

                    if (line.GetType() == typeof(TextLine))
                    {
                        TextLine textLine = (TextLine)line;

                        g.DrawStringWithShadow($"{textLine.Title}", Font, Color.White, new PointF(X, Y + counter * FontHeight + _addMonoY));
                        g.DrawStringWithShadow($"{textLine.Value}", Font, textLine.ValueBrush, new PointF(X + MaxTitleWidth + Font.Size, Y + counter * FontHeight + _addMonoY));
                    }

                    if (line.GetType() == typeof(TitledProgressBarLine))
                    {
                        TitledProgressBarLine bar = (TitledProgressBarLine)line;
                        g.DrawStringWithShadow($"{bar.Title}", Font, Brushes.White, new PointF(X, Y + counter * FontHeight));

                        ProgressBar progressBar = new ProgressBar(bar.Min, bar.Max, bar.Value);
                        progressBar.Draw(g, (int)(X + MaxTitleWidth + Font.Size), Y + counter * FontHeight + 1, (int)(MaxWidth - MaxTitleWidth - Font.Size) - 4, (int)FontHeight - 2, bar.BarColor);

                        string percent = $"{(bar.Max / bar.Value * 100):F1}%";
                        SizeF textWidth = g.MeasureString(percent, Font);
                        g.DrawStringWithShadow($"{percent}", Font, Brushes.White, new PointF((int)(X + (MaxWidth - MaxTitleWidth)) - textWidth.Width / 2, Y + counter * FontHeight + _addMonoY));
                    }

                    if (line.GetType() == typeof(CenterTextedProgressBarLine))
                    {
                        CenterTextedProgressBarLine bar = (CenterTextedProgressBarLine)line;

                        ProgressBar progressBar = new ProgressBar(bar.Min, bar.Max, bar.Value);
                        progressBar.Draw(g, X + 3, Y + counter * FontHeight + 1, (int)MaxWidth - 6, (int)FontHeight - 2, bar.BarColor);

                        SizeF textWidth = g.MeasureString(bar.CenteredText, Font);
                        g.DrawStringWithShadow($"{bar.CenteredText}", Font, Brushes.White, new PointF(X + MaxWidth / 2 - textWidth.Width / 2, Y + counter * FontHeight + _addMonoY));
                    }

                    if (line.GetType() == typeof(CenteredTextedDeltabarLine))
                    {
                        CenteredTextedDeltabarLine bar = (CenteredTextedDeltabarLine)line;

                        DeltaBar deltaBar = new DeltaBar(bar.Min, bar.Max, bar.Value);
                        deltaBar.Draw(g, X + 1, Y + counter * FontHeight + 1, (int)MaxWidth - 2, (int)FontHeight - 2);

                        SizeF textWidth = g.MeasureString(bar.CenteredText, Font);
                        g.DrawStringWithShadow($"{bar.CenteredText}", Font, Brushes.White, new PointF(X + MaxWidth / 2 - textWidth.Width / 2, Y + counter * FontHeight + _addMonoY));
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
            Lines.Add(new TextLine() { Title = title, Value = value});
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

        private class TextLine : IPanelLine
        {
            internal string Title { get; set; }
            internal string Value { get; set; }
            internal Brush ValueBrush { get; set; } = Brushes.White;
        }

        private class TitledProgressBarLine : IPanelLine
        {
            internal string Title { get; set; }
            internal double Min { get; set; }
            internal double Max { get; set; }
            internal double Value { get; set; }
            internal Brush BarColor { get; set; } = Brushes.OrangeRed;
        }

        private class CenterTextedProgressBarLine : IPanelLine
        {
            internal string CenteredText { get; set; }
            internal double Min { get; set; }
            internal double Max { get; set; }
            internal double Value { get; set; }
            internal Brush BarColor { get; set; } = Brushes.OrangeRed;
        }

        private class CenteredTextedDeltabarLine : IPanelLine
        {
            internal string CenteredText { get; set; }
            internal double Min { get; set; }
            internal double Max { get; set; }
            internal double Value { get; set; }
        }

        private interface IPanelLine { }
    }
}
