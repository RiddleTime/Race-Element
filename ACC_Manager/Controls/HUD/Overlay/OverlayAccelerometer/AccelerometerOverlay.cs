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
        private InfoPanel infoPanel = new InfoPanel(13);

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
            int paintWidth = this.Width - 1;
            int paintHeight = this.Height - 1;

            int gDotWidth = 11;
            int gDotHeight = gDotWidth;

            double AccGX = (double)pagePhysics.AccG[0] * 100;
            double AccGY = (double)pagePhysics.AccG[2] * 100;

            int gDotPosX = (int)AccGX + (paintWidth / 2) - (gDotWidth / 2);
            int gDotPosY = (int)AccGY + (paintHeight / 2) - (gDotHeight / 2);

            //Draws the HUD window
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));
            //Draws the lines and circles
            g.DrawEllipse(Pens.Red, 0, 0, paintWidth, paintHeight);
            g.DrawEllipse(Pens.LightBlue, paintWidth / 6, paintHeight / 6, (paintWidth / 3) * 2, (paintHeight / 3) * 2);
            g.DrawEllipse(Pens.Yellow, paintWidth / 3, paintHeight / 3, paintWidth / 3, paintHeight / 3);
            g.DrawLine(Pens.Red, 0, paintHeight / 2, paintWidth, paintHeight / 2);
            g.DrawLine(Pens.Red, paintWidth / 2, 0, paintWidth / 2, paintHeight);
            //Draws the 'dot'
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(0, 255, 255)),
                            new Rectangle(gDotPosX, gDotPosY, gDotWidth, gDotHeight));



            infoPanel.AddLine(new InfoPanel.InfoLine() { Title = "AccG X", Value = $"{AccGX}" });
            infoPanel.AddLine(new InfoPanel.InfoLine() { Title = "AccG Y", Value = $"{AccGY}" });
            infoPanel.Draw(g, 300);
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
