using ACCSetupApp.Properties;
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
        private readonly Font CustomFont;
        int FontHeight;

        public InfoPanel(int fontSize)
        {
            CustomFont = FontUtil.GetSpecialFont(fontSize);
            FontHeight = CustomFont.Height;
        }
        private List<InfoLine> Lines = new List<InfoLine>();

        public void AddLine(InfoLine info)
        {
            Lines.Add(info);
        }

        public void Draw(Graphics g, int maxWidth)
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
                    g.DrawString($"{line.Title}: {line.Value}", CustomFont, Brushes.White, new PointF(0, counter * FontHeight));
                    lineY += FontHeight;
                    counter++;
                    length = Lines.Count;
                }
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
