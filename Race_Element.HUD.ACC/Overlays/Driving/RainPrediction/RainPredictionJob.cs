using RaceElement.Core.Jobs.LoopJob;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.Driving.RainPrediction;

public readonly record struct RealtimeWeather
{
    public AcRainIntensity Now { get; init; }
    public AcRainIntensity In10 { get; init; }
    public AcRainIntensity In30 { get; init; }
}

internal sealed class RainPredictionJob(RainPredictionOverlay Overlay) : AbstractLoopJob
{
    private Dictionary<DateTime, AcRainIntensity> _weatherForecast = [];
    private readonly object _lockObj = new();

    private RealtimeWeather _lastWeather;
    private int _multiplier = 1;

    public Dictionary<DateTime, AcRainIntensity> WeatherForecast { get { lock (_lockObj) return _weatherForecast; } }
    public void SetMultiplier(int multiplier) { _multiplier = multiplier; }

    public sealed override void RunAction()
    {
        try
        {
            if (Overlay.pageGraphics.Status == AcStatus.AC_OFF)
            {
                ResetData();
                return;
            }

            lock (_lockObj)
            {
                RemoveOldForecast(DateTime.UtcNow);
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
            Now = Overlay.pageGraphics.rainIntensity,
            In10 = Overlay.pageGraphics.rainIntensityIn10min,
            In30 = Overlay.pageGraphics.rainIntensityIn30min
        };

        if (newScan != _lastWeather || _weatherForecast.Count == 0)
        {
            DateTime currentDateTime = DateTime.UtcNow;
            _lastWeather = newScan;

            _weatherForecast.Add(currentDateTime.AddMinutes(Get10MinutesWithMultiplier()), newScan.In10);
            _weatherForecast.Add(currentDateTime.AddMinutes(Get30MinutesWithMultiplier()), newScan.In30);

            var tmp = _weatherForecast.OrderBy(x => x.Key);
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

    private double Get10MinutesWithMultiplier()
    {
        int countDown = 10 / _multiplier;
        return countDown < 1 ? 0.5 : countDown;
    }

    private double Get30MinutesWithMultiplier()
    {
        int countDown = 30 / _multiplier;
        return countDown < 1 ? 1.0 : countDown;
    }

    internal void ResetData()
    {
        lock (_lockObj)
            _weatherForecast.Clear();
    }
}
