using ACCSetupApp.Controls.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayTrackInfo
{
    internal class TrackInfoOverlay : AbstractOverlay
    {
        private Font inputFont = new Font("Arial", 10);
        private static double weather10 = (600.0 / 48.0) / 100.0; //the 48 here represents the time multiplier in-game. ACC shows this as 0.5 minutes - x24 is same.
        private static double weather30 = (1800.0 / 48.0) / 100.0; //as above, but ACC shows this as 1 minute (x12 0.5/2 - x6 1/5 - x4 2/7 - x2 5/15 - x1 10/30)
        private static int AccWidth = 300;
        private static int AccHeight = 300;
        public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info Overlay")
        {
            this.Width = AccWidth + 1;
            this.Height = AccHeight + 1;
        }

        public override void BeforeStart() { }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            g.DrawEllipse(Pens.Red, 0, 0, 300, 300);
            g.DrawEllipse(Pens.Yellow, 75, 75, 150, 150);
            g.DrawLine(Pens.Red, 0, this.Height / 2, this.Width, this.Height / 2); // X1, Y1, X2, Y2
            g.DrawLine(Pens.Red, this.Width / 2, 0, this.Width / 2, this.Height);
            g.DrawPolygon(Pens.Red, this.Width / 3, this.Height / 3,);
            //g.DrawString($"Blinker left is on? : {pageGraphics.BlinkerLeftOn}", inputFont, Brushes.White, new PointF(0, 0));
            //g.DrawString($"")

            //g.DrawString($"Weather is : {pageGraphics.rainIntensity}", inputFont, Brushes.Yellow, new PointF(0, 20));
            //g.DrawString($"Weather in {weather10} minutes will be : {pageGraphics.rainIntensityIn10min}", inputFont, Brushes.LightBlue, new PointF(0, 40));
            //g.DrawString($"Weather in {weather30} minutes will be : {pageGraphics.rainIntensityIn30min}", inputFont, Brushes.LightGreen, new PointF(0, 60));



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
