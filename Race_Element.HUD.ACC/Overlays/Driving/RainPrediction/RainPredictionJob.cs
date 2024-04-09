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
    private Dictionary<DateTime, AcRainIntensity> _weatherForecast = [];
    private RealtimeWeather _lastWeather;

    public Dictionary<DateTime, AcRainIntensity> GetWeatherForecast()
    {
        lock (_weatherForecast)
        {
            return _weatherForecast;
        }
    }

    public override void RunAction()
    {
        try
        {
            if (Overlay.pageGraphics.Status == AcStatus.AC_OFF)
            {
                ResetData();
                return;
            }

            lock (_weatherForecast)
            {
                RemoveOldForecast(DateTime.Now);
                AddNewForecast();
            }
        }
        catch (Exception)
        {
            // let's not break something for a new release, just in case.
        }
    }

    private void AddNewForecast()
    {
        RealtimeWeather newScan = new()
        {
            Now = 0/*Overlay.pageGraphics.rainIntensity*/,
            In10 = Overlay.pageGraphics.rainIntensityIn10min,
            In30 = Overlay.pageGraphics.rainIntensityIn30min
        };

        if (newScan != _lastWeather || _weatherForecast.Count == 0)
        {
            DateTime currentDateTime = DateTime.Now;
            _lastWeather = newScan;

            _weatherForecast.Add(currentDateTime.AddMinutes(10), newScan.In10);
            _weatherForecast.Add(currentDateTime.AddMinutes(30), newScan.In30);

            // NOTE(Andrei): Order by date, and keep only the first of all consecutive equal values.
            var tmp = _weatherForecast.OrderBy(x => x.Key).GroupBy(x => x.Value).Select(x => x.First());
            _weatherForecast = tmp.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    private void RemoveOldForecast(DateTime threshold)
    {
        foreach (var kvp in _weatherForecast.Where(x => threshold.Ticks > x.Key.Ticks).ToList())
        {
            _weatherForecast.Remove(kvp.Key);
        }
    }

    internal void ResetData()
    {
        lock (_weatherForecast)
        {
            _weatherForecast.Clear();
        }
    }
}
