using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.PressureHistory;

internal readonly record struct PressureHistoryModel
{
    public string TyreCompound { get; init; }
    public int Lap { get; init; }
    public float[] Averages { get; init; }
    public float[] Min { get; init; }
    public float[] Max { get; init; }
}

internal sealed class PressureHistoryJob : AbstractLoopJob
{
    private readonly ACCOverlay _overlay;

    private readonly List<float>[] LapPressures;

    public EventHandler<PressureHistoryModel> OnNewHistory;

    public PressureHistoryJob(ACCOverlay overlay)
    {
        _overlay = overlay;
        LapPressures = new List<float>[4];
        for (int i = 0; i < 4; i++)
            LapPressures[i] = [];
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

    private PressureHistoryModel GetHistoryModel(int lap)
    {
        float[] averages = new float[4];
        float[] mins = new float[4];
        float[] maxs = new float[4];
        for (int i = 0; i < 4; i++)
        {
            averages[i] = LapPressures[i].Average();
            mins[i] = LapPressures[i].Min();
            maxs[i] = LapPressures[i].Max();
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
            LapPressures[i].Clear();
    }

    public sealed override void RunAction()
    {
        if (_overlay.DefaultShouldRender())
        {
            float[] pressures = _overlay.pagePhysics.WheelPressure;

            bool pressureExists = false;
            for (int i = 0; i < 4; i++)
                if (pressures[i] > 10)
                {
                    pressureExists = true;
                    break;
                }
            if (!pressureExists) return;

            for (int i = 0; i < 4; i++)
                LapPressures[i].Add(pressures[i]);
        }
    }
}
