using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.HUD.ACC.Data.Tracker.Weather;
using ACCManager.HUD.Overlay.Configuration;
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
        private readonly WeatherConfiguration _config = new WeatherConfiguration();
        private class WeatherConfiguration : OverlayConfiguration
        {
            public WeatherConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private readonly InfoPanel _panel;

        public WeatherOverlay(Rectangle rectangle) : base(rectangle, "Overlay Weather")
        {
            int panelWidth = 200;
            _panel = new InfoPanel(10, panelWidth);

            this.Width = panelWidth + 1;
            this.Height = _panel.FontHeight * 4;
        }

        public sealed override void BeforeStart()
        {
        }

        public sealed override void BeforeStop()
        {

        }

        public sealed override void Render(Graphics g)
        {
            _panel.AddLine("Now", WeatherTracker.Instance.Weather.Now.ToString());
            _panel.AddLine("In 10", WeatherTracker.Instance.Weather.In10.ToString());
            _panel.AddLine("In 30", WeatherTracker.Instance.Weather.In30.ToString());


            _panel.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            return false;
        }
    }
}
