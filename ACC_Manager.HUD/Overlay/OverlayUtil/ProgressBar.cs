using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Overlay.OverlayUtil
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

        public void Draw(Graphics g, int x, int y, int width, int height, Brush fillBrush, Brush outlineBrush, bool drawRounded, bool drawOutline)
        {
            Draw(g, fillBrush, outlineBrush, new Rectangle(x, y, width, height), drawRounded, drawOutline);
        }

        public void Draw(Graphics g, Brush fillBrush, Brush outlineBrush, Rectangle rectangle, bool drawRounded, bool drawOutline)
        {
            int x = rectangle.X;
            int y = rectangle.Y;
            int width = rectangle.Width;
            int height = rectangle.Height;

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double percent = Value / Max;
            if (drawRounded)
                g.FillRoundedRectangle(fillBrush, new Rectangle(x, y, (int)(width * percent), height), 1);
            else
                g.FillRectangle(fillBrush, new Rectangle(x, y, (int)(width * percent), height));

            if (drawOutline)
            {
                Brush brush = new SolidBrush(Color.White);
                if (drawRounded)
                    g.DrawRoundedRectangle(new Pen(brush), new Rectangle(x, y, width, height), 1);
                else
                    g.DrawRectangle(new Pen(brush), new Rectangle(x, y, width, height));
            }
            g.SmoothingMode = previous;
        }

    }
}
