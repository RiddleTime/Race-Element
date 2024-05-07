using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.PressureHistory;

internal readonly record struct PressureHistoryModel
{
    public float[] Averages { get; init; }
    public float[] Min { get; init; }
    public float[] Max { get; init; }
}

internal sealed class PressureHistoryJob : AbstractLoopJob
{
    private readonly AbstractOverlay _overlay;

    private List<float>[] LapPressures = new List<float>[4];

    private EventHandler<PressureHistoryModel> OnNewHistory;

    public PressureHistoryJob(AbstractOverlay overlay)
    {
        _overlay = overlay;
    }

    public sealed override void BeforeRun()
    {
        LapTracker.Instance.LapFinished += Instance_LapFinished;
    }

    private void Instance_LapFinished(object sender, RaceElement.Data.ACC.Database.LapDataDB.DbLapData e)
    {
        OnNewHistory?.Invoke(this, GetHistoryModel());
        ResetLapData();
    }

    public sealed override void AfterCancel()
    {
        LapTracker.Instance.LapFinished -= Instance_LapFinished;
    }

    private PressureHistoryModel GetHistoryModel()
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
            Averages = averages,
            Min = mins,
            Max = maxs,
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
            for (int i = 0; i < 4; i++)
                LapPressures[i].Add(_overlay.pagePhysics.WheelPressure[i]);
        }
    }
}
