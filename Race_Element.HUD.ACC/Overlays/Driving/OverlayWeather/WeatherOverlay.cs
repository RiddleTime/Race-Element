using RaceElement.Util.SystemExtensions;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.ACC.Data.Tracker.Weather;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Diagnostics;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayWeather
{
    internal class WeatherOverlay : AbstractOverlay
    {
        private readonly WeatherConfiguration _config = new();
        private sealed class WeatherConfiguration : OverlayConfiguration
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
            this.Height = _panel.FontHeight * 5;


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
                float minutesFromChange = (DateTime.Now.ToFileTimeUtc() - _weather.TimeStamp) / 10000000f / 60f;

                TimeSpan timeSpan = TimeSpan.FromSeconds(minutesFromChange * 60f);
                _panel.AddLine("last change", $"{timeSpan:mm\\:ss}");

                _panel.AddLine("Now", _weather.Now.ToString());

                float in10 = 10f / _timeMultiplier - minutesFromChange;
                in10.ClipMin(0);
                float in10secondsOnly = in10 % 1;
                in10 -= in10secondsOnly;
                in10secondsOnly *= 60;
                _panel.AddLine($"In {in10:F0}:{in10secondsOnly:F0}", _weather.In10.ToString());


                float in20 = 20f / _timeMultiplier - minutesFromChange;
                in20.ClipMin(0);
                float in20Seconds = in20 % 1;
                in20 -= in20Seconds;
                in20Seconds *= 60;
                _panel.AddLine($"In {in20:F0}:{in20Seconds:F0}", _weather.In20.ToString());

                float in30 = 30f / _timeMultiplier - minutesFromChange;
                in30.ClipMin(0);
                float in30Seconds = in30 % 1;
                in30 -= in30Seconds;
                in30Seconds *= 60;
                _panel.AddLine($"In {in30:F0}:{in30Seconds:F0}", _weather.In30.ToString());
            }

            string multiplierText = _timeMultiplier > 0 ? $"{_timeMultiplier}x" : "Detecting";
            _panel.AddLine("Multiplier", $"{multiplierText}");

            _panel.Draw(g);
        }
    }
}
