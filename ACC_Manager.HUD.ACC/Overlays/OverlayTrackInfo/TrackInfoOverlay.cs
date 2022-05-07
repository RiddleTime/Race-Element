using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayTrackInfo
{
    internal class TrackInfoOverlay : AbstractOverlay
    {
        private readonly InfoPanel panel = new InfoPanel(10, 240);

        private TrackInfoConfig config = new TrackInfoConfig();
        private class TrackInfoConfig : OverlayConfiguration
        {
            internal bool ShowGlobalFlag { get; set; } = true;
            internal bool ShowSessionType { get; set; } = true;
        }

        public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info Overlay")
        {
            this.Width = 240;
            this.Height = 54;
            RefreshRateHz = 5;
        }

        public override void BeforeStart()
        {
            if (this.config.ShowGlobalFlag)
            {
                this.Height += this.panel.FontHeight;
            }

            if (this.config.ShowSessionType)
            {
                this.Height += this.panel.FontHeight;
            }
        }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            if (this.config.ShowGlobalFlag)
                panel.AddLine("Flag", ACCSharedMemory.FlagTypeToString(pageGraphics.Flag));

            if (this.config.ShowSessionType)
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
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }


    }
}
