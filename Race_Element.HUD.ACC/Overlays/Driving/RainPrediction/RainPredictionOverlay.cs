using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Linq;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.Driving.RainPrediction;

[Overlay(Name = "Rain Prediction",
Description = "Timers that predict weather changes.",
OverlayCategory = OverlayCategory.Track,
OverlayType = OverlayType.Drive,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal sealed class RainPredictionOverlay : AbstractOverlay
{
    private RainPredictionJob _weatherJob;
    private readonly InfoPanel _panel;

    public RainPredictionOverlay(Rectangle rectangle) : base(rectangle, "Rain Prediction")
    {
        this.RefreshRateHz = 2;

        int panelWidth = 150;
        _panel = new InfoPanel(10, panelWidth);

        this.Width = panelWidth + 1;
        this.Height = _panel.FontHeight * 12;
    }

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        RaceSessionTracker.Instance.OnMultiplierChanged += Instance_OnMultiplierChanged;
        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

        _weatherJob = new RainPredictionJob(this) { IntervalMillis = 1000 };
        _weatherJob.Run();
    }

    private void Instance_OnNewSessionStarted(object sender, DbRaceSession e) => _weatherJob?.ResetData();
    private void Instance_OnMultiplierChanged(object sender, int e) => _weatherJob.SetMultiplier(e);

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        RaceSessionTracker.Instance.OnMultiplierChanged -= Instance_OnMultiplierChanged;
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;

        _weatherJob?.CancelJoin();
    }

    public sealed override void Render(Graphics g)
    {
        _panel.AddLine($"Now", $"{AcRainIntensityToString(pageGraphics.rainIntensity)}");
        AcRainIntensity prevRainIntensity = pageGraphics.rainIntensity;
        var forecast = _weatherJob.WeatherForecast.ToList();

        for (int i = 0; i < forecast.Count; ++i)
        {
            if (prevRainIntensity == forecast[i].Value)
            {
                continue;
            }

            prevRainIntensity = forecast[i].Value;
            _panel.AddLine($"{forecast[i].Key.Subtract(DateTime.UtcNow):mm\\:ss}", $"{AcRainIntensityToString(forecast[i].Value)}");
        }

        if (forecast.Count == 0)
        {
            _panel.AddLine("--:--", "No data yet");
        }

        _panel.Draw(g);
    }
}
