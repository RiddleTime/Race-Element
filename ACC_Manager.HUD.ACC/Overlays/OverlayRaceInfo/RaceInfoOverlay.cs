using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Drawing;

namespace ACCManager.HUD.ACC.Overlays.OverlayRaceInfo
{
    [Overlay(Name = "Race Info", Description = "Provides information for the current race session.", OverlayType = OverlayType.Release, Version = 1.00)]
    internal class RaceInfoOverlay : AbstractOverlay
    {
        private readonly RaceInfoConfig _config = new RaceInfoConfig();
        private class RaceInfoConfig : OverlayConfiguration
        {
            [ToolTip("Shows a timer when the pit window starts and ends.")]
            internal bool ShowPitWindow { get; set; } = true;

            [ToolTip("Shows the current location on track.")]
            internal bool ShowTrackLocation { get; set; } = true;

            public RaceInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        private readonly InfoPanel _panel = new InfoPanel(10, 240);

        public RaceInfoOverlay(Rectangle rectangle) : base(rectangle, "Race Info Overlay")
        {
            this.Width = 230;
            this.Height = _panel.FontHeight * 6;
            RefreshRateHz = 3;
        }

        public sealed override void BeforeStart()
        {
            if (!_config.ShowPitWindow)
                this.Height -= _panel.FontHeight;

            if (!_config.ShowTrackLocation)
                this.Height -= _panel.FontHeight;
        }

        public sealed override void BeforeStop()
        {
        }

        public sealed override void Render(Graphics g)
        {
            if (_config.ShowPitWindow)
                if (pageGraphics.SessionType == ACCSharedMemory.AcSessionType.AC_RACE && !pageGraphics.MandatoryPitDone)
                {
                    TimeSpan pitWindowStart = TimeSpan.FromMilliseconds(pageStatic.PitWindowStart);
                    TimeSpan pitWindowEnd = TimeSpan.FromMilliseconds(pageStatic.PitWindowEnd);

                    if (broadCastRealTime.SessionTime < pitWindowStart)
                        this._panel.AddLine("Pit Open In", $"{pitWindowStart.Subtract(broadCastRealTime.SessionTime):hh\\:mm\\:ss}");
                    else if (broadCastRealTime.SessionTime < pitWindowEnd)
                        this._panel.AddLine("Pit Closing", $"{pitWindowEnd.Subtract(broadCastRealTime.SessionTime):hh\\:mm\\:ss}");
                    else this._panel.AddLine("Pit", "Closed");
                }

            TimeSpan sessionLength = broadCastRealTime.SessionEndTime.Add(broadCastRealTime.SessionTime);


            _panel.AddLine("Position", $"{pageGraphics.Position}");
            _panel.AddLine("Laps", $"{broadCastLocalCar.Laps}");

            _panel.AddLine("Session End", $"{broadCastRealTime.SessionEndTime:hh\\:mm\\:ss}");
            _panel.AddLine("Session Time", $"{broadCastRealTime.SessionTime:hh\\:mm\\:ss}");
            _panel.AddLine("Session Length", $"{sessionLength:hh\\:mm\\:ss}");

            if (_config.ShowTrackLocation)
                this._panel.AddLine("Location", $"{broadCastLocalCar.CarLocation}");


            _panel.Draw(g);
        }

        public sealed override bool ShouldRender() => DefaultShouldRender();
    }
}
