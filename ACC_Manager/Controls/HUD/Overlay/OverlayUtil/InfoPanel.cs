﻿using ACCSetupApp.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayUtil
{
    internal class InfoPanel
    {
        private readonly Font TitleFont;
        private readonly Font ValueFont;
        int FontHeight;

        private bool MaxTitleWidthSet = false;
        private float MaxTitleWidth = 0;

        public InfoPanel(int fontSize)
        {
            TitleFont = FontUtil.GetBoldFont(fontSize);
            ValueFont = FontUtil.GetLightFont(fontSize);
            FontHeight = TitleFont.Height;
        }
        private List<InfoLine> Lines = new List<InfoLine>();

        public void AddLine(InfoLine info)
        {
            Lines.Add(info);
        }

        public void Draw(Graphics g)
        {
            int lineY = 0;
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

                    if (!MaxTitleWidthSet)
                    {
                        SizeF titleWidth;
                        if ((titleWidth = g.MeasureString(line.Title, TitleFont)).Width > MaxTitleWidth)
                        {
                            MaxTitleWidth = titleWidth.Width;
                        }
                    }
                    else
                    {
                        g.DrawString($"{line.Title}", TitleFont, Brushes.White, new PointF(0, counter * FontHeight));
                        g.DrawString($"{line.Value}", ValueFont, Brushes.White, new PointF(MaxTitleWidth + TitleFont.Size, counter * FontHeight));
                        lineY += FontHeight;
                    }

                    counter++;
                    length = Lines.Count;
                }

                if (!MaxTitleWidthSet)
                    MaxTitleWidthSet = true;
            }
            g.TextRenderingHint = previousHint;

            lock (Lines)
                Lines.Clear();
        }



        public class InfoLine
        {
            internal string Title { get; set; }
            internal string Value { get; set; }
        }
    }
}
