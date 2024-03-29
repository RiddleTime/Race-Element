using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.ACC.Overlays.Driving.Weather;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.OverlayRainPrediction;

[Overlay(Name = "Rain Prediction",
Description = "Shows predicted rain forecasts.",
OverlayCategory = OverlayCategory.Track,
OverlayType = OverlayType.Drive,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal sealed class RainPredictionOverlay : AbstractOverlay
{
    private readonly WeatherConfiguration _config = new();
    private sealed class WeatherConfiguration : OverlayConfiguration
    {
        public WeatherConfiguration() => this.GenericConfiguration.AllowRescale = true;
    }

    private RainPredictionJob _weatherJob;

    private readonly InfoPanel _panel;
    private int _timeMultiplier = -1;

    public RainPredictionOverlay(Rectangle rectangle) : base(rectangle, "Rain Prediction")
    {
        this.RefreshRateHz = 1;
        int panelWidth = 200;
        _panel = new InfoPanel(10, panelWidth);

        this.Width = panelWidth + 1;
        this.Height = _panel.FontHeight * 20;
    }

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        SessionTimeTracker.Instance.OnMultiplierChanged += Instance_OnMultiplierChanged;
        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

        _weatherJob = new RainPredictionJob(this) { IntervalMillis = 1000 };
        _weatherJob.Run();
    }

    private void Instance_OnNewSessionStarted(object sender, DbRaceSession e) => _weatherJob?.ResetData();
    private void Instance_OnMultiplierChanged(object sender, int e) => _timeMultiplier = e;

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        SessionTimeTracker.Instance.OnMultiplierChanged -= Instance_OnMultiplierChanged;
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;

        _weatherJob.CancelJoin();
    }

    public sealed override void Render(Graphics g)
    {
        if (_weatherJob != null)
        {
            _panel.AddLine($"Now", $"{AcRainIntensityToString(pageGraphics.rainIntensity)}");

            List<KeyValuePair<DateTime, AcRainIntensity>> data;
            lock (_weatherJob.UpcomingChanges)
            {
                data = [.. _weatherJob.UpcomingChanges.Where(x => x.Key > DateTime.UtcNow).OrderBy(x => x.Key)];
            }

            if (data.Count == 0)
            {
                _panel.AddLine("--:--", "No data yet");
            }
            else
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (i < data.Count - 1 && i > 0)
                        if (data[i - 1].Value == data[i].Value && data[i + 1].Value == data[i].Value)
                            continue;

                    _panel.AddLine($"{data[i].Key.Subtract(DateTime.UtcNow):mm\\:ss}", $"{AcRainIntensityToString(data[i].Value)}");
                }
            }
        }

        //string multiplierText = _timeMultiplier > 0 ? $"{_timeMultiplier}x" : "Detecting";
        //_panel.AddLine("Multiplier", $"{multiplierText}");

        _panel.Draw(g);
    }
}
