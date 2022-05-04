using ACCSetupApp.Controls.HUD.Overlay.Internal;
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


            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            g.DrawEllipse(Pens.Red, 0, 0, paintWidth, paintHeight);
            g.DrawEllipse(Pens.Yellow, 75, 75, paintWidth / 2, paintHeight / 2);
            g.DrawLine(Pens.Red, 0, paintHeight / 2, paintWidth, paintHeight / 2); // X1, Y1, X2, Y2
            g.DrawLine(Pens.Red, paintWidth / 2, 0, paintWidth / 2, paintHeight);

            // commented this since it's not compiling :P 
            //g.DrawPolygon(Pens.Red, this.Width / 3, this.Height / 3,);
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
