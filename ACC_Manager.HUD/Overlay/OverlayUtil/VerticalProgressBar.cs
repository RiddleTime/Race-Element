using System.Drawing;
using System.Drawing.Drawing2D;

namespace ACCManager.HUD.Overlay.OverlayUtil
{
    public class VerticalProgressBar
    {
        internal double Min { get; set; }
        internal double Max { get; set; }
        internal double Value { get; set; }

        public VerticalProgressBar(double min, double max, double value)
        {
            Min = min;
            Max = max;
            Value = value;
        }

        public void Draw(Graphics g, int x, int y, int width, int height)
        {
            this.Draw(g, x, y, width, height, Brushes.OrangeRed, Brushes.White);
        }

        public void Draw(Graphics g, int x, int y, int width, int height, Brush fillbrush, Brush outlineBrush, bool rounded = false)
        {
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double percent = Value / Max;

            if (rounded)
            {
                if (percent >= 0.03f)
                    g.FillRoundedRectangle(fillbrush, new Rectangle(x, y + height - (int)(height * percent), width, (int)(height * percent)), 3);
                g.DrawRoundedRectangle(new Pen(outlineBrush), new Rectangle(x, y, width, height), 3);
            }
            else
            {
                g.FillRectangle(fillbrush, new Rectangle(x, y + height - (int)(height * percent), width, (int)(height * percent)));
                g.DrawRectangle(new Pen(outlineBrush), new Rectangle(x, y, width, height));
            }
            g.SmoothingMode = previous;
        }
    }
}
