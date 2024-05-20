using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Core;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Tracker;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaceElement.Data.ACC.Session;

public readonly record struct GapPointData
{
    public DateTime PassedAt { get; init; } = DateTime.MinValue;

    public GapPointData()
    {
    }
}

internal static class GapTrackerConstants
{
    public const int GapDistanceMeter = 100;
    public const int MeasuringInterval = 1000 / 100;
}

public sealed class GapTracker : AbstractLoopJob
{
    private readonly ConcurrentDictionary<int, GapPointData[]> GapData = [];
    private int TotalGaps = 0;

    public float TimeGapBetween(int currentCarIndex, float splineCurrent, int carAheadIndex)
    {
        if (!GapData.TryGetValue(currentCarIndex, out GapPointData[] gapsA) || !GapData.TryGetValue(carAheadIndex, out GapPointData[] gapsB))
            return 0;

        int estimatedIndex = (int)(TotalGaps * splineCurrent);
        estimatedIndex.ClipMax(TotalGaps - 1);
        DateTime passedAtA = gapsA[estimatedIndex].PassedAt;
        DateTime passedAtB = gapsB[estimatedIndex].PassedAt;
        if (passedAtA == DateTime.MinValue || passedAtB == DateTime.MinValue)
            return 0;
        TimeSpan gap = passedAtA - passedAtB;
        return (float)gap.TotalSeconds;
    }

    public static readonly GapTracker Instance = new()
    {
        IntervalMillis = GapTrackerConstants.MeasuringInterval
    };

    public GapTracker()
    {
    }

    public override void BeforeRun()
    {
        BroadcastTracker.Instance.OnTrackDataUpdate += Instance_OnTrackDataUpdate;
    }

    public override void AfterCancel()
    {
        BroadcastTracker.Instance.OnTrackDataUpdate -= Instance_OnTrackDataUpdate;
    }

    private void Instance_OnTrackDataUpdate(object sender, Broadcast.Structs.TrackData e)
    {
        GapData.Clear();
        TotalGaps = (int)Math.Floor(e.TrackMeters / GapTrackerConstants.GapDistanceMeter);
        Debug.WriteLine($"Received track distance, total gaps: {TotalGaps} at {GapTrackerConstants.GapDistanceMeter} meters per gap.");
    }

    public override void RunAction()
    {
        if (EntryListTracker.Instance.Cars.Count == 0 || !AccProcess.IsRunning)
        {
            if (GapData.IsEmpty) GapData.Clear();
            return;
        }

        Parallel.ForEach(EntryListTracker.Instance.Cars, entry =>
        {
            if (!GapData.TryGetValue(entry.Key, out GapPointData[] data))
            {
                data = new GapPointData[TotalGaps];
                GapData.TryAdd(entry.Key, data);
            }

            float gapStep = 1f / TotalGaps;
            for (int i = 0; i < TotalGaps; i++)
            {
                float spline = entry.Value.RealtimeCarUpdate.SplinePosition;
                float estimatedGapSpline = i * gapStep;
                if (spline > estimatedGapSpline && spline < estimatedGapSpline + gapStep * 1.5f)
                {
                    if (data[i].PassedAt == DateTime.MinValue || DateTime.UtcNow > data[i].PassedAt.AddMinutes(1))
                        data[i] = new GapPointData() { PassedAt = DateTime.UtcNow };
                }
            }

        });
    }
}
