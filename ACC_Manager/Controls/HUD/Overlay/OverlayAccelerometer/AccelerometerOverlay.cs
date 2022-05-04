using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayAccelerometer
{
    internal class AccelerometerOverlay : AbstractOverlay
    {
        private InfoPanel infoPanel = new InfoPanel(8);

        public AccelerometerOverlay(Rectangle rectangle) : base(rectangle, "Accelerometer Overlay")
        {
            this.Width = 300;
            this.Height = this.Width;
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop()
        {
        }


        public override void Render(Graphics g)
        {
            //Draws the HUD window
            g.FillRectangle(new SolidBrush(Color.FromArgb(140, Color.Black)), new Rectangle(0, 0, this.Width, this.Height));

            DrawGMeter(48, 48, 250, g);

            double xPercentage = GetPercentage(3, pagePhysics.AccG[0]);
            double yPercentage = GetPercentage(3, pagePhysics.AccG[2]);

            infoPanel.AddLine(new InfoPanel.InfoLine() { Title = "AccG X", Value = $"{pagePhysics.AccG[0].ToString("F2")}" });
            infoPanel.AddLine(new InfoPanel.InfoLine() { Title = "AccG Y", Value = $"{pagePhysics.AccG[2].ToString("F2")}" });
            infoPanel.AddLine(new InfoPanel.InfoLine() { Title = "X percent", Value = $"{(xPercentage * 100).ToString("F2")}" });
            infoPanel.AddLine(new InfoPanel.InfoLine() { Title = "Y percent", Value = $"{(yPercentage * 10).ToString("F2")}" });

            infoPanel.Draw(g);
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

            float maxG = 2.5f;
            double xPercentage = GetPercentage(maxG, pagePhysics.AccG[0]);
            double yPercentage = GetPercentage(maxG, pagePhysics.AccG[2]);

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
