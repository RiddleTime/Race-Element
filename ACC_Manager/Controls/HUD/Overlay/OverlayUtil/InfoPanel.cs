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
        Font RobotoFont = new Font(FontUtil.GetRobotoMedium(), 15);
        int FontHeight;

        public InfoPanel()
        {
            FontHeight = RobotoFont.Height;
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
            foreach (InfoLine line in Lines)
            {
                g.DrawString($"{line.Title}: {line.Value}", RobotoFont, Brushes.White, new PointF(0, lineY));
                lineY += FontHeight;
            }
            g.TextRenderingHint = previousHint;
        }



        public class InfoLine
        {
            internal string Title { get; set; }
            internal string Value { get; set; }
        }
    }
}
