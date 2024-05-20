using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    public const int TotalGaps = 100;
    public const int MeasuringInterval = 1000 / 100;
}

public sealed class GapTracker : AbstractLoopJob
{
    private readonly ConcurrentDictionary<int, GapPointData[]> GapData = [];

    public float TimeGapBetween(int currentCarIndex, float splineCurrent, int carAheadIndex)
    {
        if (!GapData.TryGetValue(currentCarIndex, out GapPointData[] gapsA) || !GapData.TryGetValue(carAheadIndex, out GapPointData[] gapsB))
            return 0;

        int estimatedIndex = (int)(GapTrackerConstants.TotalGaps * splineCurrent);
        estimatedIndex.ClipMax(GapTrackerConstants.TotalGaps - 1);
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

    public override void RunAction()
    {
        if (EntryListTracker.Instance.Cars.Count == 0) return;

        Parallel.ForEach(EntryListTracker.Instance.Cars, entry =>
        {
            if (!GapData.TryGetValue(entry.Key, out GapPointData[] data))
            {
                data = new GapPointData[GapTrackerConstants.TotalGaps];
                GapData.TryAdd(entry.Key, data);
            }

            float gapStep = 1f / GapTrackerConstants.TotalGaps;
            for (int i = 0; i < GapTrackerConstants.TotalGaps; i++)
            {
                if (entry.Value.RealtimeCarUpdate.SplinePosition > i * gapStep)
                {
                    if (data[i].PassedAt == DateTime.MinValue || DateTime.UtcNow > data[i].PassedAt.AddMinutes(1))
                        data[i] = new GapPointData() { PassedAt = DateTime.UtcNow };
                }
            }

        });
    }
}
