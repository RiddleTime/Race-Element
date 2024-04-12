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
        if (_rainJob != null)
        {
            _panel.AddLine($"Now", $"{AcRainIntensityToString(pageGraphics.rainIntensity)}");

            List<KeyValuePair<DateTime, AcRainIntensity>> data;
            lock (_rainJob.UpcomingChanges)
                data = [.. _rainJob.UpcomingChanges.Where(x => x.Key > DateTime.UtcNow).OrderBy(x => x.Key)];

            if (data.Count != 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (i == 0 && data[i].Value == pageGraphics.rainIntensity) continue;

                    if (i > 0)
                    {
                        if (i < data.Count - 1)
                            if (data[i - 1].Value == data[i].Value && data[i + 1].Value == data[i].Value)
                                continue;

                        if (data[i - 1].Value == data[i].Value)
                            continue;
                    }

                    _panel.AddLine($"{data[i].Key.Subtract(DateTime.UtcNow):mm\\:ss}", $"{AcRainIntensityToString(data[i].Value)}");
                }
            }

#if DEBUG
            _panel.AddLine("Multiplier", $"{_rainJob.Multiplier}X");
#endif
        }


        _panel.Draw(g);
    }
}
