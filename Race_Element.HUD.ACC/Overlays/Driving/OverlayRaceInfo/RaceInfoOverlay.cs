using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayRaceInfo;

[Overlay(Name = "Race Info", Description = "(BETA) Provides information for the current race session.", OverlayType = OverlayType.Drive, Version = 1.00)]
internal class RaceInfoOverlay : AbstractOverlay
{
    private readonly RaceInfoConfig _config = new();
    private class RaceInfoConfig : OverlayConfiguration
    {
        [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping InfoPanel { get; init; } = new InfoPanelGrouping();
        public class InfoPanelGrouping
        {
            [ToolTip("Shows a timer when the pit window starts and ends.")]
            public bool PitWindow { get; init; } = true;

            [ToolTip("Shows the current location on track.")]
            public bool TrackLocation { get; init; } = true;
        }

        public RaceInfoConfig()
        {
            this.AllowRescale = true;
        }
    }

    private readonly InfoPanel _panel = new(10, 230);

    public RaceInfoOverlay(Rectangle rectangle) : base(rectangle, "Race Info")
    {
        this.Width = 230;
        this.Height = _panel.FontHeight * 6;
        RefreshRateHz = 1;
    }

    public sealed override void BeforeStart()
    {
        if (!_config.InfoPanel.PitWindow)
            this.Height -= _panel.FontHeight;

        if (!_config.InfoPanel.TrackLocation)
            this.Height -= _panel.FontHeight;
    }

    public sealed override void Render(Graphics g)
    {
        if (_config.InfoPanel.PitWindow)
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

        if (_config.InfoPanel.TrackLocation)
            this._panel.AddLine("Location", $"{broadCastLocalCar.CarLocation}");


        _panel.Draw(g);
    }
}
