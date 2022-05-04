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
            DrawGMeter(48, 48, 250, g);

            infoPanel.AddLine(new InfoPanel.InfoLine() { Title = "AccG X", Value = $"{pagePhysics.AccG[0]}" });
            infoPanel.AddLine(new InfoPanel.InfoLine() { Title = "AccG Y", Value = $"{pagePhysics.AccG[2]}" });
            infoPanel.Draw(g);
        }

        private void DrawGMeter(int x, int y, int size, Graphics g)
        {
            int gMeterX = x;
            int gMeterY = y;
            int paintWidth = size;
            int paintHeight = size;

            Pen AccPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1);
            Pen AccPen2 = new Pen(Color.FromArgb(30, 255, 255, 255), 3);
            Pen AccPen3 = new Pen(Color.FromArgb(100, 255, 255, 255), 4);
            Pen AccPen4 = new Pen(Color.FromArgb(200, 200, 200, 200), 5);

            int gDotSize = 14;

            double AccGX = (double)pagePhysics.AccG[0] * 100;
            double AccGY = (double)pagePhysics.AccG[2] * 100;

            int gDotPosX = (int)AccGX + (paintWidth / 2) - (gDotSize / 2);
            int gDotPosY = (int)AccGY + (paintHeight / 2) - (gDotSize / 2);

            //Draws the HUD window
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));
            //Draws the lines and circles
            g.DrawLine(AccPen, 0 + gMeterX, gMeterY + paintHeight / 2, gMeterX + paintWidth, gMeterY + paintHeight / 2);
            g.DrawLine(AccPen, gMeterX + paintWidth / 2, gMeterY, gMeterX + paintWidth / 2, gMeterY + paintHeight);
            System.Drawing.Drawing2D.SmoothingMode previousSmoothing = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawEllipse(AccPen4, gMeterX + 2, gMeterY + 2, paintWidth - 4, paintHeight - 4);
            g.DrawEllipse(AccPen3, gMeterX + paintWidth / 6, gMeterY + paintHeight / 6, (paintWidth / 3) * 2, (paintHeight / 3) * 2);
            g.DrawEllipse(AccPen2, gMeterX + paintWidth / 3, gMeterY + paintHeight / 3, paintWidth / 3, paintHeight / 3);

            //Draws the 'dot'
            g.FillEllipse(new SolidBrush(System.Drawing.Color.FromArgb(242, 82, 2)),
                            new Rectangle(gMeterX + gDotPosX, gMeterY + gDotPosY, gDotSize, gDotSize));
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
