using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
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
    private RainPredictionConfiguration _config = new();

    private RainPredictionJob _rainJob;
    private readonly InfoPanel _panel;
    private KeyValuePair<DateTime, AcRainIntensity>[] _rainPredictionData;

    public RainPredictionOverlay(Rectangle rectangle) : base(rectangle, "Rain Prediction")
    {
        int panelWidth = 150;
        _panel = new InfoPanel(10, panelWidth);
        _rainPredictionData = [];

        this.Width = panelWidth + 1;
        this.Height = _panel.FontHeight * 12;
        this.RefreshRateHz = 2;
    }

    public sealed override void SetupPreviewData()
    {
        pageGraphics.rainIntensity = AcRainIntensity.Drizzle;
        _rainPredictionData = [
            new(DateTime.UtcNow.AddMinutes(1), AcRainIntensity.Drizzle),
            new(DateTime.UtcNow.AddMinutes(2), AcRainIntensity.Drizzle),
            new(DateTime.UtcNow.AddMinutes(3), AcRainIntensity.Drizzle),
            new(DateTime.UtcNow.AddMinutes(4), AcRainIntensity.Drizzle),
            new(DateTime.UtcNow.AddMinutes(6), AcRainIntensity.Light_Rain),
            new(DateTime.UtcNow.AddMinutes(6.5d), AcRainIntensity.Medium_Rain),
            new(DateTime.UtcNow.AddMinutes(6.8d), AcRainIntensity.Medium_Rain),
            new(DateTime.UtcNow.AddMinutes(6.9d), AcRainIntensity.Medium_Rain),
            new(DateTime.UtcNow.AddMinutes(8), AcRainIntensity.Light_Rain),
            new(DateTime.UtcNow.AddMinutes(9.7d), AcRainIntensity.Light_Rain),
            new(DateTime.UtcNow.AddMinutes(17.3d), AcRainIntensity.No_Rain),
            new(DateTime.UtcNow.AddMinutes(19.3d), AcRainIntensity.No_Rain),
        ];
    }

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStarted;

        _rainJob = new RainPredictionJob(this) { IntervalMillis = 1000 };
        _rainJob.Run();
    }

    private void OnNewSessionStarted(object sender, DbRaceSession e) => _rainJob?.ResetData();

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        RaceSessionTracker.Instance.OnNewSessionStarted -= OnNewSessionStarted;

        _rainJob?.CancelJoin();
    }

    public sealed override void Render(Graphics g)
    {
        _panel.AddLine($"Now", $"{AcRainIntensityToString(pageGraphics.rainIntensity)}");

        if (!IsPreviewing && _rainJob != null)
            lock (_rainJob.UpcomingChanges)
                _rainPredictionData = [.. _rainJob.UpcomingChanges.Where(x => x.Key > DateTime.UtcNow).OrderBy(x => x.Key)];

        if (_rainPredictionData.Length != 0)
        {
            ReadOnlySpan<KeyValuePair<DateTime, AcRainIntensity>> spanData = _rainPredictionData.AsSpan();

            for (int i = 0; i < spanData.Length; i++)
            {
                KeyValuePair<DateTime, AcRainIntensity> prediction = spanData[i];
                if (i == 0 && prediction.Value == pageGraphics.rainIntensity) continue;

                if (i > 0)
                {
                    if (i < spanData.Length - 1)
                        if (spanData[i - 1].Value == prediction.Value && spanData[i + 1].Value == prediction.Value)
                            continue;

                    if (spanData[i - 1].Value == prediction.Value)
                        continue;
                }

                _panel.AddLine($"{prediction.Key.Subtract(DateTime.UtcNow):mm\\:ss}", $"{AcRainIntensityToString(prediction.Value)}");
            }
        }
        //_panel.AddLine("Multiplier", $"{_rainJob.Multiplier}X");   // used this in the future for showing live multiplier data.

        _panel.Draw(g);
    }
}
