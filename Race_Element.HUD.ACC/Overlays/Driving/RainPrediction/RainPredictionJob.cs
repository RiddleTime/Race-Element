using RaceElement.Core.Jobs.LoopJob;
using RaceElement.HUD.ACC.Overlays.OverlayRainPrediction;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.Driving.Weather;

public record struct RealtimeWeather
{
    public RealtimeWeather() { }
    public AcRainIntensity Now { get; set; }
    public AcRainIntensity In10 { get; set; }
    public AcRainIntensity In30 { get; set; }
}

internal sealed class RainPredictionJob(RainPredictionOverlay Overlay) : AbstractLoopJob
{
    private readonly Dictionary<DateTime, RealtimeWeather> WeatherChanges = [];
    public readonly Dictionary<DateTime, AcRainIntensity> UpcomingChanges = [];
    private RealtimeWeather _lastWeather;

    public override void RunAction()
    {
        try
        {
            if (Overlay.pageGraphics.Status == AcStatus.AC_OFF)
            {
                ResetData();
                return;
            }

            lock (UpcomingChanges)
                if (UpcomingChanges.Count > 200)
                    for (int i = 0; i < 100; i++)
                        UpcomingChanges.Remove(UpcomingChanges.Keys.First());

            if (WeatherChanges.Count == 0)
                _lastWeather = new()
                {
                    Now = Overlay.pageGraphics.rainIntensity,
                    In10 = Overlay.pageGraphics.rainIntensityIn10min,
                    In30 = Overlay.pageGraphics.rainIntensityIn30min
                };

            RealtimeWeather newScan = new()
            {
                Now = Overlay.pageGraphics.rainIntensity,
                In10 = Overlay.pageGraphics.rainIntensityIn10min,
                In30 = Overlay.pageGraphics.rainIntensityIn30min
            };

            if (newScan != _lastWeather || UpcomingChanges.Count == 0)
            {
                DateTime change = DateTime.UtcNow;
                WeatherChanges.Add(change, newScan);
                lock (UpcomingChanges)
                {
                    UpcomingChanges.Add(change.AddMinutes(10), newScan.In10);
                    UpcomingChanges.Add(change.AddMinutes(30), newScan.In30);
                }
                _lastWeather = newScan;
            }
        }
        catch (Exception)
        {
            // let's not break something for a new release, just in case.
        }
    }

    internal void ResetData()
    {
        lock (WeatherChanges)
            WeatherChanges.Clear();
        lock (UpcomingChanges)
            UpcomingChanges.Clear();
        return;
    }
}
