using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.TyreTempHistory;

internal readonly record struct TyreTempHistoryModel
{
    public string TyreCompound { get; init; }
    public int Lap { get; init; }
    public float[] Averages { get; init; }
    public float[] Min { get; init; }
    public float[] Max { get; init; }
}

internal sealed class TyreTempHistoryJob : AbstractLoopJob
{
    private readonly AbstractOverlay _overlay;

    private readonly List<float>[] LapTemps;

    public EventHandler<TyreTempHistoryModel> OnNewHistory;

    public TyreTempHistoryJob(AbstractOverlay overlay)
    {
        _overlay = overlay;
        LapTemps = new List<float>[4];
        for (int i = 0; i < 4; i++)
            LapTemps[i] = [];
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

    private TyreTempHistoryModel GetHistoryModel(int lap)
    {
        float[] averages = new float[4];
        float[] mins = new float[4];
        float[] maxs = new float[4];
        for (int i = 0; i < 4; i++)
        {
            averages[i] = LapTemps[i].Average();
            mins[i] = LapTemps[i].Min();
            maxs[i] = LapTemps[i].Max();
        }

        return new()
        {
            Lap = lap,
            Averages = averages,
            Min = mins,
            Max = maxs,
            TyreCompound = _overlay.pageGraphics.TyreCompound
        };
    }

    private void ResetLapData()
    {
        for (int i = 0; i < 4; i++)
            LapTemps[i].Clear();
    }

    public sealed override void RunAction()
    {
        if (_overlay.DefaultShouldRender())
        {
            float[] pressures = _overlay.pagePhysics.TyreCoreTemperature;

            bool tempExists = false;
            for (int i = 0; i < 4; i++)
                if (pressures[i] > 10)
                {
                    tempExists = true;
                    break;
                }
            if (!tempExists) return;

            for (int i = 0; i < 4; i++)
                LapTemps[i].Add(pressures[i]);
        }
    }
}
