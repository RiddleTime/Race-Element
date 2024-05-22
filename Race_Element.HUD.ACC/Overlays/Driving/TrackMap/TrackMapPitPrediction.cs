
using System;
using System.Collections.Generic;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public static class TrackMapPitPrediction
{
    public static PitStop GetPitStop(List<TrackPoint> track, float pitTimeMs)
    {
        var name = ACCSharedMemory.Instance.PageFileStatic.Track.ToLower();
        var info = TrackInfo.Data.GetValueOrDefault(name, new TrackInfo(0, 0, 0));

        return ComputePitStop(track, pitTimeMs + info.PitLaneTimeMs);
    }

    public static PitStop GetPitStopWithDamage(List<TrackPoint> track, float pitTimeMs)
    {
        var p = ACCSharedMemory.Instance.PageFilePhysics;
        var time = RaceElement.Data.ACC.Cars.Damage.GetTotalRepairTime(p);

        if (time == 0)
        {
            return null;
        }

        var name = ACCSharedMemory.Instance.PageFileStatic.Track.ToLower();
        var info = TrackInfo.Data.GetValueOrDefault(name, new TrackInfo(0, 0, 0));

        time = time * 1000 + pitTimeMs + info.PitLaneTimeMs;
        var pitStop = ComputePitStop(track, time);

        if (pitStop != null)
        {
            pitStop.Damage = true;
        }

        return pitStop;
    }

    private static PitStop ComputePitStop(List<TrackPoint> track, float pitTime)
    {
        var pageFileGraphic = ACCSharedMemory.Instance.PageFileGraphic;
        var estimatedLapTime = pageFileGraphic.EstimatedLapTimeMillis;
        var currenTime = pageFileGraphic.CurrentTimeMs;

        if (estimatedLapTime == Int32.MaxValue)
        {
            return null;
        }

        var diffTime = currenTime - (pitTime % estimatedLapTime);
        diffTime = diffTime >= 0 ? diffTime : estimatedLapTime + diffTime;

        var delta = diffTime / estimatedLapTime;
        var p = track.BinarySearch(new TrackPoint() { Spline = delta }, new TrackPointSplineComparator());

        if (p < 0)
        {
            return null;
        }

        var result = new PitStop
        {
            Position = track[p],
            Laps = (int)(pitTime / pageFileGraphic.BestTimeMs)
        };

        return result;
    }
}
