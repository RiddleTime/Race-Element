
using System;
using System.Collections.Generic;
using RaceElement.Data.ACC.Tracks;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public static class TrackMapPitPrediction
{
    public static PitStop GetPitStop(List<TrackPoint> track, float pitTimeMs)
    {
        var info = TrackData.GetCurrentTrack(ACCSharedMemory.Instance.PageFileStatic.Track);
        return ComputePitStop(track, pitTimeMs + info.PitLaneTime);
    }

    public static PitStop GetPitStopWithDamage(List<TrackPoint> track, float pitTimeMs)
    {
        var p = ACCSharedMemory.Instance.PageFilePhysics;
        var time = RaceElement.Data.ACC.Cars.Damage.GetTotalRepairTime(p);

        if (time == 0)
        {
            return null;
        }

        var info = TrackData.GetCurrentTrack(ACCSharedMemory.Instance.PageFileStatic.Track);
        time = time * 1000 + pitTimeMs + info.PitLaneTime;
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
