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

        public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info Overlay")
        {
            this.Width = 200;
            this.Height = 100;
        }

        public override void BeforeStart() { }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

          InfoDrawing infoDrawing = new InfoDrawing();

            g.DrawString($"Blinker left is on? : {pageGraphics.BlinkerLeftOn}", inputFont, Brushes.White, new PointF(0, 0));
            //g.DrawString($"")
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            return false;
        }

        internal class InfoDrawing
        {
            internal List<InfoObject> Infos = new List<InfoObject>();
        }

        internal class InfoObject
        {
            internal string Header;
            internal string Info;
        }
    }
}
