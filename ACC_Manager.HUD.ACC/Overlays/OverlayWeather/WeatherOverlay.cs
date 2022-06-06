using ACCManager.Data.ACC.Session;
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
        private WeatherInfo _weather;
        private int _timeMultiplier = -1;

        public WeatherOverlay(Rectangle rectangle) : base(rectangle, "Overlay Weather")
        {
            this.RefreshRateHz = 1;
            int panelWidth = 200;
            _panel = new InfoPanel(10, panelWidth);

            this.Width = panelWidth + 1;
            this.Height = _panel.FontHeight * 4;


        }

        private void Instance_OnMultiplierChanged(object sender, int e)
        {
            _timeMultiplier = e;
        }

        private void Instance_OnWeatherChanged(object sender, WeatherInfo e)
        {
            Debug.WriteLine("Weather has changed");
            _weather = e;
        }

        public sealed override void BeforeStart()
        {
            SessionTimeTracker.Instance.OnMultiplierChanged += Instance_OnMultiplierChanged;
            WeatherTracker.Instance.OnWeatherChanged += Instance_OnWeatherChanged;
        }

        public sealed override void BeforeStop()
        {
            SessionTimeTracker.Instance.OnMultiplierChanged -= Instance_OnMultiplierChanged;
            WeatherTracker.Instance.OnWeatherChanged -= Instance_OnWeatherChanged;
        }

        public sealed override void Render(Graphics g)
        {
            if (_weather != null && _timeMultiplier > 0)
            {
                _panel.AddLine("Now", _weather.Now.ToString());

                float in10 = 10f / _timeMultiplier;
                float in10secondsOnly = in10 % 1;
                in10 -= in10secondsOnly;
                in10secondsOnly *= 60;
                _panel.AddLine($"In {in10:F0}:{in10secondsOnly:F0}", _weather.In10.ToString());

                float in30 = 30f / _timeMultiplier;
                float in30Seconds = in30 % 1;
                in30 -= in30Seconds;
                in30Seconds *= 60;
                _panel.AddLine($"In {in30:F0}:{in30Seconds:F0}", _weather.In30.ToString());
            }

            string multiplierText = _timeMultiplier > 0 ? $"{_timeMultiplier}x" : "Detecting";
            _panel.AddLine("Multiplier", $"{multiplierText}");

            _panel.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
