using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayRaceInfo
{
    [Overlay(Name = "Race Info", Description = "Description TODO!!!!", OverlayType = OverlayType.Release, Version = 1.00)]
    internal class RaceInfoOverlay : AbstractOverlay
    {
        private readonly RaceInfoConfig _config = new RaceInfoConfig();
        private class RaceInfoConfig : OverlayConfiguration
        {
            public RaceInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        private readonly InfoPanel _panel = new InfoPanel(10, 240);

        public RaceInfoOverlay(Rectangle rectangle) : base(rectangle, "Race Info Overlay")
        {
            this.Width = 230;
            this.Height = _panel.FontHeight * 3;
            RefreshRateHz = 5;
        }

        public sealed override void BeforeStart()
        {
        }

        public sealed override void BeforeStop()
        {
        }

        public sealed override void Render(Graphics g)
        {
            TimeSpan pitWindowStart = TimeSpan.FromMilliseconds(pageStatic.PitWindowStart);
            TimeSpan pitWindowEnd = TimeSpan.FromMilliseconds(pageStatic.PitWindowEnd);
            this._panel.AddLine("Start", $"{pitWindowStart:hh\\:mm\\:ss}");
            this._panel.AddLine("End", $"{pitWindowEnd:hh\\:mm\\:ss}");

            this._panel.AddLine("Session End", $"{broadCastRealTime.SessionEndTime:hh\\:mm\\:ss}");


            _panel.Draw(g);
        }

        public sealed override bool ShouldRender() => DefaultShouldRender();
    }
}
