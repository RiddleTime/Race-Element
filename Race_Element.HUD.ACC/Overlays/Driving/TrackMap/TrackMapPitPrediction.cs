
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public class PitStop
{
    public PointF Position;
    public int Laps;
}

public class TrackMapPitPrediction
{
    private static float  _defaultPitTimeMs = 30 * 1000;

    private static Dictionary<string, float> _trackPitLineTime = new Dictionary<string, float>
    {
        { "barcelona"       , 30 * 1000 },
        { "bathurst"        , 19 * 1000 },
        { "brands_hatch"    , 19 * 1000 },
        { "cota"            , 29 * 1000 },
        { "donington"       , 19 * 1000 },
        { "hungaroring"     , 24 * 1000 },
        { "imola"           , 38 * 1000 },
        { "indianapolis"    , 44 * 1000 },
        { "kyalami"         , 18 * 1000 },
        { "laguna_seca"     , 21 * 1000 },
        { "misano"          , 28 * 1000 },
        { "monza"           , 31 * 1000 },
        { "nurburgring"     , 25 * 1000 },
        { "nurburgring_24h" , 25 * 1000 },
        { "oulton_park"     , 14 * 1000 },
        { "paul_ricard"     , 27 * 1000 },
        { "silverstone"     , 23 * 1000 },
        { "snetterton"      , 19 * 1000 },
        { "spa"             , 57 * 1000 },
        { "suzuka"          , 27 * 1000 },
        { "valencia"        , 39 * 1000 },
        { "watkins_glen"    , 27 * 1000 },
        { "zandvoort"       , 19 * 1000 },
        { "zolder"          , 30 * 1000 }
    };

    public static PitStop GetPitStop(List<PointF> track)
    {
        float pitLaneTime = 0;
        var name = ACCSharedMemory.Instance.PageFileStatic.Track.ToLower();

        _trackPitLineTime.TryGetValue(name, out pitLaneTime);
        return ComputePitStop(track, _defaultPitTimeMs + pitLaneTime);
    }

    public static PitStop GetPitStopWithDamage(List<PointF> track)
    {
        var a = ACCSharedMemory.Instance.PageFilePhysics.CarDamage[0];
        var b = ACCSharedMemory.Instance.PageFilePhysics.CarDamage[1];
        var c = ACCSharedMemory.Instance.PageFilePhysics.CarDamage[2];
        var d = ACCSharedMemory.Instance.PageFilePhysics.CarDamage[3];
        var e = ACCSharedMemory.Instance.PageFilePhysics.CarDamage[4];
        var result = a + b + c + d + e;

        if (result == 0)
        {
            return null;
        }

        float pitLaneTime = 0;
        var name = ACCSharedMemory.Instance.PageFileStatic.Track.ToLower();

        _trackPitLineTime.TryGetValue(name, out pitLaneTime);
        result = result * 0.142f * 1000 + _defaultPitTimeMs + pitLaneTime;

        return ComputePitStop(track, result);
    }

    private static PitStop ComputePitStop(List<PointF> track, float time)
    {
        var pageFileGraphic = ACCSharedMemory.Instance.PageFileGraphic;
        var currenTime = pageFileGraphic.CurrentTimeMs;
        var bestTime = pageFileGraphic.BestTimeMs;

        if (bestTime == Int32.MaxValue)
        {
            return null;
        }

        var diffTime = currenTime - (time % bestTime);
        diffTime = diffTime >= 0 ? diffTime : bestTime + diffTime;

        var delta = diffTime / bestTime;
        var trackPos = (int)(delta * track.Count);

        var result = new PitStop
        {
            Position = track[trackPos],
            Laps = (int)(time / pageFileGraphic.BestTimeMs)
        };

        return result;
    }
}
