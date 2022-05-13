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

            if(Value < Average)
            {
                g.FillRectangle(negativeBrush, new Rectangle(x, y, (int)(width * percent), height));
            }

            if(Value == Average)
            {

            }

            if(Value > Average)
            {

            }


           

            Brush brush = new SolidBrush(Color.White);
            g.DrawRectangle(new Pen(brush), new Rectangle(x, y, width, height));

            g.SmoothingMode = previous;
        }

    }
}
