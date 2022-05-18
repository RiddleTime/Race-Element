using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.HUD.ACC.Data.Tracker.Weather;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayWeather
{
    internal class WeatherOverlay : AbstractOverlay
    {
        private WeatherConfiguration config = new WeatherConfiguration();
        private class WeatherConfiguration : OverlayConfiguration
        {
            public WeatherConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private InfoPanel panel;

        public WeatherOverlay(Rectangle rectangle) : base(rectangle, "Overlay Weather")
        {
            int panelWidth = 200;
            panel = new InfoPanel(10, panelWidth);

            this.Width = panelWidth + 1;
            this.Height = panel.FontHeight * 4;
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop()
        {

        }

        public override void Render(Graphics g)
        {
            panel.AddLine("Now", WeatherTracker.Instance.Weather.Now.ToString());
            panel.AddLine("In 10", WeatherTracker.Instance.Weather.In10.ToString());
            panel.AddLine("In 30", WeatherTracker.Instance.Weather.In30.ToString());
            panel.AddLine("Rain Level", $"{broadCastRealTime.TrackTemp}");
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
