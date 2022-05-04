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
        Font Font = new Font(FontUtil.GetRobotoMedium(), 15);

        private List<InfoLine> Lines = new List<InfoLine>();

        public void AddLine(InfoLine info)
        {
            Lines.Add(info);
        }

        public void Draw(Graphics g, int maxWidth)
        {

            // pack://application:,,,/MaterialDesignThemes.Wpf;component/Resources/#Roboto-Medium

            int lineY = 0;
            TextRenderingHint previousHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.TextContrast = 2;
            foreach (InfoLine line in Lines)
            {
                g.DrawString($"{line.Title}: {line.Value}", Font, Brushes.White, new PointF(0, lineY));
                lineY += Font.Height;
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
