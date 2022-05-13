using ACC_Manager.Util.NumberExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.OverlayUtil
{
    internal class DeltaBar
    {

        internal double Min { get; set; }
        internal double Max { get; set; }
        internal double Value { get; set; }

        private double Average { get; set; }

        public DeltaBar(double min, double max, double value)
        {
            Min = min;
            Max = max;
            Value = value.Clip(Min, Max);
            Average = (Min + Max) / 2;
        }

        public void Draw(Graphics g, int x, int y, int width, int height)
        {
            this.Draw(g, x, y, width, height, Brushes.LimeGreen, Brushes.OrangeRed);
        }

        public void Draw(Graphics g, int x, int y, int width, int height, Brush negativeBrush, Brush positiveBrush)
        {
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double percent = Value / Max;

            Color outlineColor = Color.White;

            if (Value < Average)
            {
                double range = Average + Min;
                double relativeValue = Value - Min;
                if (range < 0) range *= -1;
                double fillPercent = relativeValue / range;

                int fillWidth = width / 2 - (int)(width / 2 * fillPercent);
                g.FillRectangle(negativeBrush, new Rectangle(width / 2 - fillWidth, y, fillWidth, height));
            }

            if (Value == Average)
            {

            }

            if (Value > Average)
            {
                double range = Average + Max;
                double relativeValue = Max - Value;
                if (range < 0) range *= -1;
                double fillPercent = relativeValue / range;

                int fillWidth = width / 2 - (int)(width / 2 * fillPercent);
                g.FillRectangle(positiveBrush, new Rectangle(width / 2, y, fillWidth, height));
            }

            Brush brush = new SolidBrush(outlineColor);
            g.DrawRectangle(new Pen(brush), new Rectangle(x, y, width, height));

            g.SmoothingMode = previous;
        }

    }
}
