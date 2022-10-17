using ACC_Manager.Util.SystemExtensions;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ACCManager.HUD.Overlay.OverlayUtil
{
    public class DeltaBar
    {

        internal double Min { get; set; }
        internal double Max { get; set; }
        internal double Value { get; set; }

        private double Average { get; set; }

        public bool DrawBackground = false;
        public bool IsValidLap = false;
        public bool DrawOutline = true;
        public Color PositiveColor = Color.OrangeRed;
        public Color NegativeColor = Color.LimeGreen;

        public DeltaBar(double min, double max, double value)
        {
            Min = min;
            Max = max;
            Value = value.Clip(Min, Max);
            Average = (Min + Max) / 2;
        }

        public void Draw(Graphics g, int x, int y, int width, int height)
        {
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double percent = Value / Max;

            Color drawingColor = Color.White;

            if (DrawBackground) { 
                if (IsValidLap)
                    g.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.Black)), new Rectangle(x, y, width, height));
                else
                    g.FillRectangle(new SolidBrush(Color.FromArgb(120, Color.DarkRed)), new Rectangle(x, y, width, height));
            }

            if (Value < Average)
            {
                drawingColor = NegativeColor;

                double range = Average + Min;
                double relativeValue = Value - Min;
                if (range < 0) range *= -1;
                double fillPercent = relativeValue / range;

                int fillWidth = width / 2 - (int)(width / 2 * fillPercent);
                g.FillRectangle(new SolidBrush(drawingColor), new Rectangle(width / 2 - fillWidth, y, fillWidth, height));
            }

            if (Value == Average)
            {

            }

            if (Value > Average)
            {
                drawingColor = PositiveColor;

                double range = Average + Max;
                double relativeValue = Max - Value;
                if (range < 0) range *= -1;
                double fillPercent = relativeValue / range;

                int fillWidth = width / 2 - (int)(width / 2 * fillPercent);
                g.FillRectangle(new SolidBrush(drawingColor), new Rectangle(width / 2, y, fillWidth, height));
            }

            if (DrawOutline)
            {
                Brush outlineBrush = new SolidBrush(drawingColor);
                
                if (!IsValidLap)
                    outlineBrush = new SolidBrush(Color.DarkRed);

                g.DrawRectangle(new Pen(outlineBrush), new Rectangle(x, y, width, height));
            }

            g.SmoothingMode = previous;
        }

    }
}
