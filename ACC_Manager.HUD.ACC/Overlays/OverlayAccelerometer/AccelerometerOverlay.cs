using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayAccelerometer
{
    internal class AccelerometerOverlay : AbstractOverlay
    {
        private InfoPanel info = new InfoPanel(8);
        private const int MaxG = 3;
        private int gMeterX = 48;
        private int gMeterY = 48;



        private AccelleroConfig config = new AccelleroConfig();
        private class AccelleroConfig : OverlayConfiguration
        {
            internal bool ShowText { get; set; } = true;
        }

        public AccelerometerOverlay(Rectangle rectangle) : base(rectangle, "Accelerometer Overlay")
        {
            this.Width = 300;
            this.Height = this.Width;
        }

        public override void BeforeStart()
        {
            if (!this.config.ShowText)
            {
                this.Width = 251;
                this.Height = this.Width;
                gMeterX = 0;
                gMeterY = 0;
            }
        }
        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            //Draws the HUD window
            g.FillRectangle(new SolidBrush(Color.FromArgb(140, Color.Black)), new Rectangle(0, 0, this.Width, this.Height));


            DrawGMeter(gMeterX, gMeterY, 250, g);

            double xPercentage = GetPercentage(MaxG, pagePhysics.AccG[0]);
            double yPercentage = GetPercentage(MaxG, pagePhysics.AccG[2]);

            if (this.config.ShowText)
            {
                info.AddLine("AccG X", $"{pagePhysics.AccG[0]:F2}");
                info.AddLine("AccG Y", $"{pagePhysics.AccG[2]:F2}");
                info.AddLine("X percent", $"{xPercentage * 100:F2}");
                info.AddLine("Y percent", $"{yPercentage * 100:F2}");

                info.Draw(g);
            }
        }


        /// <summary>
        /// Returns a percentage, mininum -100% and max 100%
        /// </summary>
        /// <param name="max"></param>
        /// <param name="actual"></param>
        /// <returns>a value between -1 and 1 (inclusive)</returns>
        public float GetPercentage(float max, float actual)
        {
            float percentage = actual * 100 / max / 100;

            if (percentage > 1)
                percentage = 1;
            if (percentage < -1)
                percentage = -1;

            return percentage;
        }

        private void DrawGMeter(int x, int y, int size, Graphics g)
        {
            int gMeterX = x;
            int gMeterY = y;

            //Draws the lines and circles
            Pen AccPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1);
            Pen AccPen2 = new Pen(Color.FromArgb(30, 255, 255, 255), 3);
            Pen AccPen3 = new Pen(Color.FromArgb(100, 255, 255, 255), 4);
            Pen AccPen4 = new Pen(Color.FromArgb(200, 200, 200, 200), 5);

            g.DrawLine(AccPen, 0 + gMeterX, gMeterY + size / 2, gMeterX + size, gMeterY + size / 2);
            g.DrawLine(AccPen, gMeterX + size / 2, gMeterY, gMeterX + size / 2, gMeterY + size);
            System.Drawing.Drawing2D.SmoothingMode previousSmoothing = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawEllipse(AccPen4, gMeterX + 2, gMeterY + 2, size - 4, size - 4);
            g.DrawEllipse(AccPen3, gMeterX + size / 6, gMeterY + size / 6, (size / 3) * 2, (size / 3) * 2);
            g.DrawEllipse(AccPen2, gMeterX + size / 3, gMeterY + size / 3, size / 3, size / 3);

            //Draws the 'dot'
            int gDotSize = 14;

            double xPercentage = GetPercentage(MaxG, pagePhysics.AccG[0]);
            double yPercentage = GetPercentage(MaxG, pagePhysics.AccG[2]);

            PointF middle = new PointF(x + size / 2, y + size / 2);
            int gDotPosX = (int)(middle.X + (size * 0.8 / 2 * xPercentage) - (gDotSize / 2));
            int gDotPosY = (int)(middle.Y + (size * 0.8 / 2 * yPercentage) - (gDotSize / 2));

            g.FillEllipse(new SolidBrush(Color.FromArgb(242, 82, 2)), new Rectangle(gDotPosX, gDotPosY, gDotSize, gDotSize));

            g.SmoothingMode = previousSmoothing;
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            return false;
        }
    }
}
