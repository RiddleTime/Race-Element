using ACCManager.HUD.Overlay.Configuration;
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
    internal sealed class TrackInfoOverlay : AbstractOverlay
    {
        private readonly InfoPanel _panel = new InfoPanel(10, 240);

        private readonly TrackInfoConfig _config = new TrackInfoConfig();
        private class TrackInfoConfig : OverlayConfiguration
        {
            [ToolTip("Shows the global track flag.")]
            internal bool ShowGlobalFlag { get; set; } = true;

            [ToolTip("Shows the type of the session.")]
            internal bool ShowSessionType { get; set; } = true;

            [ToolTip("Displays the actual time on track.")]
            internal bool ShowTimeOfDay { get; set; } = true;

            public TrackInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info Overlay")
        {
            this.Width = 230;
            this.Height = _panel.FontHeight * 6; ;
            RefreshRateHz = 5;
        }

        public sealed override void BeforeStart()
        {
            if (!this._config.ShowGlobalFlag)
                this.Height -= this._panel.FontHeight;

            if (!this._config.ShowSessionType)
                this.Height -= this._panel.FontHeight;

            if (!this._config.ShowTimeOfDay)
                this.Height -= this._panel.FontHeight;
        }

        public sealed override void BeforeStop() { }

        public sealed override void Render(Graphics g)
        {
            if (this._config.ShowTimeOfDay)
            {
                TimeSpan time = TimeSpan.FromMilliseconds(broadCastRealTime.TimeOfDay.TotalMilliseconds * 1000);
                this._panel.AddLine("Time", $"{time:hh\\:mm\\:ss}");
            }

            if (this._config.ShowGlobalFlag)
                _panel.AddLine("Flag", ACCSharedMemory.FlagTypeToString(pageGraphics.Flag));

            if (this._config.ShowSessionType)
                _panel.AddLine("Session", ACCSharedMemory.SessionTypeToString(pageGraphics.SessionType));

            _panel.AddLine("Grip", pageGraphics.trackGripStatus.ToString());
            string airTemp = Math.Round(pagePhysics.AirTemp, 2).ToString("F2");
            string roadTemp = Math.Round(pagePhysics.RoadTemp, 2).ToString("F2");
            _panel.AddLine("Air - Track", $"{airTemp} C - {roadTemp} C");
            _panel.AddLine("Wind", $"{Math.Round(pageGraphics.WindSpeed, 2)} km/h");
            _panel.Draw(g);
        }

        public sealed override bool ShouldRender()
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
