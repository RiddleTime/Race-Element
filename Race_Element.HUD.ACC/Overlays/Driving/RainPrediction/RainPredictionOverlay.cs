using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.ACC.Overlays.Driving.Weather;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.OverlayRainPrediction;

[Overlay(Name = "Rain Prediction",
Description = "Timers that predict weather changes.",
OverlayCategory = OverlayCategory.Track,
OverlayType = OverlayType.Drive,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal sealed class RainPredictionOverlay : AbstractOverlay
{
    private sealed class WeatherConfiguration : OverlayConfiguration
    {
        public WeatherConfiguration() => this.GenericConfiguration.AllowRescale = true;
    }

    private readonly WeatherConfiguration _config = new();
    private RainPredictionJob _weatherJob;

    private readonly InfoPanel _panel;
    //private int _timeMultiplier = -1;

    public RainPredictionOverlay(Rectangle rectangle) : base(rectangle, "Rain Prediction")
    {
        this.RefreshRateHz = 2;
        int panelWidth = 200;
        _panel = new InfoPanel(10, panelWidth);

        this.Width = panelWidth + 1;
        this.Height = _panel.FontHeight * 12;
    }

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        //SessionTimeTracker.Instance.OnMultiplierChanged += Instance_OnMultiplierChanged;
        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

        _weatherJob = new RainPredictionJob(this) { IntervalMillis = 1000 };
        _weatherJob.Run();
    }

    private void Instance_OnNewSessionStarted(object sender, DbRaceSession e) => _weatherJob?.ResetData();
    //private void Instance_OnMultiplierChanged(object sender, int e) => _timeMultiplier = e;

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        //SessionTimeTracker.Instance.OnMultiplierChanged -= Instance_OnMultiplierChanged;
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;

        _weatherJob?.CancelJoin();
    }

    public sealed override void Render(Graphics g)
    {
        if (_weatherJob != null)
        {
            _panel.AddLine($"Now", $"{AcRainIntensityToString(pageGraphics.rainIntensity)}");
            var weather = _weatherJob.GetWeatherForecast();

            if (weather.Count == 0)
            {
                _panel.AddLine("--:--", "No data yet");
                return;
            }

            foreach (var kvp in weather)
            {
                _panel.AddLine($"{kvp.Key.Subtract(DateTime.Now):mm\\:ss}", $"{AcRainIntensityToString(kvp.Value)}");
            }
        }

        //string multiplierText = _timeMultiplier > 0 ? $"{_timeMultiplier}x" : "Detecting";
        //_panel.AddLine("Multiplier", $"{multiplierText}");

        _panel.Draw(g);
    }
}
