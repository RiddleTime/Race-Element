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
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double percent = Max / Value;
            if (percent > 0.03)
                g.FillRoundedRectangle(Brushes.OrangeRed, new Rectangle(x, y, (int)(width * percent), height), 3);

            Brush brush = new SolidBrush(Color.White);
            g.DrawRoundedRectangle(new Pen(brush), new Rectangle(x, y, width, height), 3);

            g.SmoothingMode = previous;
        }
    }
}
