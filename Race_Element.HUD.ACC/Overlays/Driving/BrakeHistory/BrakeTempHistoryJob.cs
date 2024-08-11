using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.BrakeHistory;

internal readonly record struct BrakeTempHistoryModel
{
    public int Lap { get; init; }
    public float[] Averages { get; init; }
    public float[] Min { get; init; }
    public float[] Max { get; init; }
}

internal sealed class BrakeTempHistoryJob : AbstractLoopJob
{
    private readonly AbstractOverlay _overlay;

    private readonly List<float>[] LapBrakeTemps;

    public EventHandler<BrakeTempHistoryModel> OnNewHistory;

    public BrakeTempHistoryJob(AbstractOverlay overlay)
    {
        _overlay = overlay;
        LapBrakeTemps = new List<float>[4];
        for (int i = 0; i < 4; i++)
            LapBrakeTemps[i] = [];
    }

    public sealed override void BeforeRun()
    {
        LapTracker.Instance.LapFinished += OnLapFinished;
        RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStarted;
    }

    private void OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
    {
        ResetLapData();
    }

    private void OnLapFinished(object sender, RaceElement.Data.ACC.Database.LapDataDB.DbLapData e)
    {
        OnNewHistory?.Invoke(this, GetHistoryModel(e.Index));
        ResetLapData();
    }

    public sealed override void AfterCancel()
    {
        LapTracker.Instance.LapFinished -= OnLapFinished;
        RaceSessionTracker.Instance.OnNewSessionStarted -= OnNewSessionStarted;
    }

    private BrakeTempHistoryModel GetHistoryModel(int lap)
    {
        float[] averages = new float[4];
        float[] mins = new float[4];
        float[] maxs = new float[4];
        for (int i = 0; i < 4; i++)
        {
            averages[i] = LapBrakeTemps[i].Average();
            mins[i] = LapBrakeTemps[i].Min();
            maxs[i] = LapBrakeTemps[i].Max();
        }

        return new()
        {
            Lap = lap,
            Averages = averages,
            Min = mins,
            Max = maxs,
        };
    }

    private void ResetLapData()
    {
        for (int i = 0; i < 4; i++)
            LapBrakeTemps[i].Clear();
    }

    public sealed override void RunAction()
    {
        if (_overlay.DefaultShouldRender())
        {
            float[] temperatures = _overlay.pagePhysics.BrakeTemperature;

            bool tempExists = false;
            for (int i = 0; i < 4; i++)
                if (temperatures[i] > 10)
                {
                    tempExists = true;
                    break;
                }
            if (!tempExists) return;

            for (int i = 0; i < 4; i++)
                LapBrakeTemps[i].Add(temperatures[i]);
        }
    }
}

