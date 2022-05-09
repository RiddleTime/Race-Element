using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.OverlayUtil
{
    internal class ProgressBar
    {
        internal double Min { get; set; }
        internal double Max { get; set; }
        internal double Value { get; set; }

        public ProgressBar(double min, double max, double value)
        {
            Min = min;
            Max = max;
            Value = value;
        }

        public void Draw(Graphics g, int x, int y, int width, int height)
        {
            this.Draw(g, x, y, width, height, Brushes.OrangeRed);
        }

        public void Draw(Graphics g, int x, int y, int width, int height, Brush fillbrush)
        {
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double percent = Value / Max;
            g.FillRectangle(fillbrush, new Rectangle(x, y, (int)(width * percent), height));

            Brush brush = new SolidBrush(Color.White);
            g.DrawRectangle(new Pen(brush), new Rectangle(x, y, width, height));

            g.SmoothingMode = previous;
        }
    }
}
