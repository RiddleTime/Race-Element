using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Controls.HUD.Overlay.OverlayTrackInfo
{
    internal class TrackInfoOverlay : AbstractOverlay
    {
        private readonly InfoPanel panel = new InfoPanel(10);

        public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info Overlay")
        {
            this.Width = 240;
            this.Height = 86;
            RefreshRateHz = 5;
        }

        public override void BeforeStart() { }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            panel.AddLine("Flag", ACCSharedMemory.FlagTypeToString(pageGraphics.Flag));
            panel.AddLine("Session", ACCSharedMemory.SessionTypeToString(pageGraphics.SessionType));
            panel.AddLine("Grip", pageGraphics.trackGripStatus.ToString());
            string airTemp = Math.Round(pagePhysics.AirTemp, 2).ToString("F2");
            string roadTemp = Math.Round(pagePhysics.RoadTemp, 2).ToString("F2");
            panel.AddLine("Air - Track", $"{airTemp} C - {roadTemp} C");
            panel.AddLine("Wind", $"{Math.Round(pageGraphics.WindSpeed, 2)} km/h");
            panel.Draw(g);
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
