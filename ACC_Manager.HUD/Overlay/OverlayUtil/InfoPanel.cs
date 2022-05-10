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

using ACCManager.HUD.Overlay.OverlayUtil;


namespace ACCManager.HUD.Overlay.Util
{
    public class InfoPanel
    {
        private readonly Font TitleFont;
        private readonly Font ValueFont;

        private readonly int MaxWidth;
        public int X = 0;
        public int Y = 0;

        private int _fontHeight;
        public int FontHeight { get { return this._fontHeight; } private set { this._fontHeight = value; } }
        public bool DrawBackground = true;


        private bool MaxTitleWidthSet = false;
        private float MaxTitleWidth = 0;

        public InfoPanel(double fontSize, int maxWidth)
        {
            this.MaxWidth = maxWidth;
            this.TitleFont = FontUtil.GetBoldFont((float)fontSize);
            this.ValueFont = FontUtil.GetLightFont((float)fontSize);
            this.FontHeight = TitleFont.Height;
        }
        private List<InfoLine> Lines = new List<InfoLine>();

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
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(140, 0, 0, 0)), new Rectangle(X, Y, this.MaxWidth, Lines.Count * this.FontHeight), 6);
                g.SmoothingMode = previous;
            }

            TextRenderingHint previousHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.TextContrast = 2;

            lock (Lines)
            {
                int length = Lines.Count;
                int counter = 0;
                while (counter < length)
                {
                    InfoLine line = Lines[counter];

                    if (line.GetType() == typeof(TextLine))
                    {
                        TextLine textLine = (TextLine)line;
                        g.DrawString($"{textLine.Title}", TitleFont, Brushes.White, new PointF(X, Y + counter * FontHeight));
                        g.DrawString($"{textLine.Value}", ValueFont, Brushes.White, new PointF(X + MaxTitleWidth + TitleFont.Size, Y + counter * FontHeight));
                    }

                    if (line.GetType() == typeof(TitledProgressBarLine))
                    {
                        TitledProgressBarLine bar = (TitledProgressBarLine)line;
                        g.DrawString($"{bar.Title}", TitleFont, Brushes.White, new PointF(X, Y + counter * FontHeight));

                        ProgressBar progressBar = new ProgressBar(bar.Min, bar.Max, bar.Value);
                        progressBar.Draw(g, (int)(X + MaxTitleWidth + TitleFont.Size), Y + counter * FontHeight + 1, (int)(MaxWidth - MaxTitleWidth - TitleFont.Size) - 2, (int)FontHeight - 2);

                        string percent = $"{(bar.Max / bar.Value * 100):F1}%";
                        SizeF textWidth = g.MeasureString(percent, TitleFont);
                        g.DrawString($"{percent}", TitleFont, Brushes.White, new PointF((int)(X + (MaxWidth - MaxTitleWidth)) - textWidth.Width / 2, Y + counter * FontHeight));
                    }

                    if (line.GetType() == typeof(CenterTextedProgressBarLine))
                    {
                        CenterTextedProgressBarLine bar = (CenterTextedProgressBarLine)line;

                        ProgressBar progressBar = new ProgressBar(bar.Min, bar.Max, bar.Value);
                        progressBar.Draw(g, X + 1, Y + counter * FontHeight + 1, (int)MaxWidth - 2, (int)FontHeight - 2);

                        SizeF textWidth = g.MeasureString(bar.CenteredText, TitleFont);
                        g.DrawString($"{bar.CenteredText}", TitleFont, Brushes.White, new PointF(X + MaxWidth / 2 - textWidth.Width / 2, Y + counter * FontHeight));

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
                    InfoLine line = Lines[counter];


                    if (line.GetType() == typeof(TextLine))
                    {
                        TextLine textLine = (TextLine)line;
                        SizeF titleWidth;
                        if ((titleWidth = g.MeasureString(textLine.Title, TitleFont)).Width > MaxTitleWidth)
                            MaxTitleWidth = titleWidth.Width;
                    }

                    if (line.GetType() == typeof(TitledProgressBarLine))
                    {
                        TitledProgressBarLine titledProgressBar = (TitledProgressBarLine)line;
                        SizeF titleWidth;
                        if ((titleWidth = g.MeasureString(titledProgressBar.Title, TitleFont)).Width > MaxTitleWidth)
                            MaxTitleWidth = titleWidth.Width;
                    }


                    counter++;
                    length = Lines.Count;
                }
            }
        }


        public void AddLine(string title, string value)
        {
            Lines.Add(new TextLine() { Title = title, Value = value });
        }

        public void AddProgressBar(string title, double min, double max, double value)
        {
            Lines.Add(new TitledProgressBarLine() { Title = title, Min = min, Max = max, Value = value });
        }

        public void AddProgressBarWithCenteredText(string centeredText, double min, double max, double value)
        {
            Lines.Add(new CenterTextedProgressBarLine() { CenteredText = centeredText, Min = min, Max = max, Value = value });
        }

        private class TextLine : InfoLine
        {
            internal string Title { get; set; }
            internal string Value { get; set; }
        }

        private class TitledProgressBarLine : InfoLine
        {
            internal string Title { get; set; }
            internal double Min { get; set; }
            internal double Max { get; set; }
            internal double Value { get; set; }
        }

        private class CenterTextedProgressBarLine : InfoLine
        {
            internal string CenteredText { get; set; }
            internal double Min { get; set; }
            internal double Max { get; set; }
            internal double Value { get; set; }
        }

        private interface InfoLine { }
    }
}
