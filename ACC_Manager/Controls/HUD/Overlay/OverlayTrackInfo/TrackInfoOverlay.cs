using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.Controls.HUD.Overlay.OverlayUtil.InfoPanel;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayTrackInfo
{
    internal class TrackInfoOverlay : AbstractOverlay
    {
        private readonly InfoPanel panel = new InfoPanel(11);

        public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info Overlay")
        {
            this.Width = 320;
            this.Height = 93;
        }

        public override void BeforeStart() { }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            panel.AddLine(new InfoLine() { Title = "Flag", Value = ACCSharedMemory.FlagTypeToString(pageGraphics.Flag) });
            panel.AddLine(new InfoLine() { Title = "Session", Value = ACCSharedMemory.SessionTypeToString(pageGraphics.SessionType) });
            panel.AddLine(new InfoLine() { Title = "Grip", Value = pageGraphics.trackGripStatus.ToString() });
            panel.AddLine(new InfoLine() { Title = "Temp (Air/Track)", Value = $"{Math.Round(pagePhysics.AirTemp, 2)} C - {Math.Round(pagePhysics.RoadTemp, 2)} C" });
            panel.AddLine(new InfoLine() { Title = "Wind", Value = $"{Math.Round(pageGraphics.WindSpeed, 2)} km/h" });
            panel.Draw(g);
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
