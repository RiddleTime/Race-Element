using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayAccelerometer
{
    internal class AccelerometerOverlay : AbstractOverlay
    {
        private AccelleroConfig config = new AccelleroConfig();
        private class AccelleroConfig : OverlayConfiguration
        {
            internal bool ShowTrace { get; set; } = true;
            internal bool ShowText { get; set; } = false;
        }

        private InfoPanel info = new InfoPanel(8, 225) { DrawBackground = false };
        private const int MaxG = 3;
        private int gMeterX = 22;
        private int gMeterY = 22;
        private int gMeterSize = 200;
        private LinkedList<Point> trace = new LinkedList<Point>();

        public AccelerometerOverlay(Rectangle rectangle) : base(rectangle, "Accelerometer Overlay")
        {
            this.Width = 225;
            this.Height = this.Width;
            this.RefreshRateHz = 20;
        }

        public override void BeforeStart()
        {
            if (!this.config.ShowText)
            {
                this.Width = gMeterSize + 1;
                this.Height = this.Width;
                gMeterX = 0;
                gMeterY = 0;
            }
        }
        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(140, Color.Black));
            //Draws the HUD window
            if (this.config.ShowText)
                g.FillRectangle(backgroundBrush, new Rectangle(0, 0, this.Width, this.Height));
            else
                g.FillEllipse(backgroundBrush, new Rectangle(1, 1, this.Width - 2, this.Height - 2));

            DrawGMeter(gMeterX, gMeterY, gMeterSize, g);

            double xPercentage = GetPercentage(MaxG, pagePhysics.AccG[0]);
            double yPercentage = GetPercentage(MaxG, pagePhysics.AccG[2]);

            if (this.config.ShowText)
            {
                info.AddLine("AccG X", $"{pagePhysics.AccG[0]:F2}");
                info.AddLine("AccG Y", $"{pagePhysics.AccG[2]:F2}");

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

            percentage = KeepBetween(percentage, -1, 1);
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


            SmoothingMode previousSmoothing = g.SmoothingMode;
            //Draws the lines and circles
            Pen AccPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1);
            Pen AccPen2 = new Pen(Color.FromArgb(30, 255, 255, 255), 3);
            Pen AccPen3 = new Pen(Color.FromArgb(100, 255, 255, 255), 4);
            Pen AccPen4 = new Pen(Color.FromArgb(200, 200, 200, 200), 5);

            g.DrawLine(AccPen, 0 + gMeterX, gMeterY + size / 2, gMeterX + size, gMeterY + size / 2);
            g.DrawLine(AccPen, gMeterX + size / 2, gMeterY, gMeterX + size / 2, gMeterY + size);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.DrawEllipse(AccPen4, gMeterX + 2, gMeterY + 2, size - 4, size - 4);
            g.DrawEllipse(AccPen3, gMeterX + size / 6, gMeterY + size / 6, (size / 3) * 2, (size / 3) * 2);
            g.DrawEllipse(AccPen2, gMeterX + size / 3, gMeterY + size / 3, size / 3, size / 3);


            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //Draws the 'dot'
            int gDotSize = 14;

            double xPercentage = GetPercentage(MaxG, pagePhysics.AccG[0]);
            double yPercentage = GetPercentage(MaxG, pagePhysics.AccG[2]);

            double direction = Math.Atan2(xPercentage, yPercentage);
            double magnitude = Math.Sqrt(xPercentage * xPercentage + yPercentage * yPercentage);
            magnitude = KeepBetween(magnitude, -1, 1);

            double horizontalPlacement = Math.Sin(direction) * magnitude;
            double verticalPlacement = Math.Cos(direction) * magnitude;
            verticalPlacement = KeepBetween(verticalPlacement, -1, 1);
            horizontalPlacement = KeepBetween(horizontalPlacement, -1, 1);

            PointF middle = new PointF(x + size / 2, y + size / 2);
            int gDotPosX = (int)(middle.X + (size / 2 * horizontalPlacement) - (gDotSize / 2));
            int gDotPosY = (int)(middle.Y + (size / 2 * verticalPlacement) - (gDotSize / 2));

            g.FillEllipse(new SolidBrush(Color.FromArgb(242, 82, 2)), new Rectangle(gDotPosX, gDotPosY, gDotSize, gDotSize));

            if (this.config.ShowTrace)
            {
                lock (trace)
                {
                    trace.AddFirst(new Point(gDotPosX, gDotPosY));
                    if (trace.Count > 10)
                        trace.RemoveLast();


                    for (int i = 0; i < trace.Count; i++)
                    {
                        Point traceItem = trace.ElementAt(i);
                        g.FillEllipse(new SolidBrush(Color.FromArgb(90 - i * 5, 242, 82, 2)), new Rectangle(traceItem.X, traceItem.Y, gDotSize, gDotSize));
                    }
                }
            }

            g.SmoothingMode = previousSmoothing;
        }

        public double KeepBetween(double value, double min, double max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        public float KeepBetween(float value, float min, float max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            bool shouldRender = true;
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }
    }
}
