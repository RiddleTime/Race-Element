using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayTrackInfo
{
    [Overlay(Name = "Track Info", Version = 1.00, OverlayType = OverlayType.Release,
        Description = "A panel showing information about the track state: grip, temperatures and wind.\nOptionally showing the global flag, session type and time of day.")]
    internal sealed class TrackInfoOverlay : AbstractOverlay
    {
        private readonly TrackInfoConfig _config = new TrackInfoConfig();
        private class TrackInfoConfig : OverlayConfiguration
        {
            [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
            public InfoPanelGrouping InfoPanel { get; set; } = new InfoPanelGrouping();
            public class InfoPanelGrouping
            {
                [ToolTip("Shows the global track flag.")]
                public bool GlobalFlag { get; set; } = true;

                [ToolTip("Shows the type of the session.")]
                public bool SessionType { get; set; } = true;

                [ToolTip("Displays the actual time on track.")]
                public bool TimeOfDay { get; set; } = true;
            }

            public TrackInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        private readonly InfoPanel _panel = new InfoPanel(10, 240);

        public TrackInfoOverlay(Rectangle rectangle) : base(rectangle, "Track Info")
        {
            this.Width = 230;
            this.Height = _panel.FontHeight * 6; ;
            RefreshRateHz = 5;
        }

        public sealed override void BeforeStart()
        {
            if (!this._config.InfoPanel.GlobalFlag)
                this.Height -= this._panel.FontHeight;

            if (!this._config.InfoPanel.SessionType)
                this.Height -= this._panel.FontHeight;

            if (!this._config.InfoPanel.TimeOfDay)
                this.Height -= this._panel.FontHeight;
        }

        public sealed override void BeforeStop() { }

        public sealed override void Render(Graphics g)
        {
            if (this._config.InfoPanel.TimeOfDay)
            {
                TimeSpan time = TimeSpan.FromMilliseconds(broadCastRealTime.TimeOfDay.TotalMilliseconds * 1000);
                this._panel.AddLine("Time", $"{time:hh\\:mm\\:ss}");
            }

            if (this._config.InfoPanel.GlobalFlag)
                _panel.AddLine("Flag", ACCSharedMemory.FlagTypeToString(pageGraphics.Flag));

            if (this._config.InfoPanel.SessionType)
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
            return DefaultShouldRender();
        }
    }
}
