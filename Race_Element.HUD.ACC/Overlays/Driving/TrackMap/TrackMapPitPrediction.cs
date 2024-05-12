
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public class PitStop
{
    public PointF Position;
    public bool Damage;
    public int Laps;
}

public class TrackMapPitPrediction
{
    private static readonly float DefaultPitTimeMs = 30_000;

    private static readonly Dictionary<string, float> TrackPitLineTimeMs = new Dictionary<string, float>
    {
        { "barcelona"       , 30_000 },
        { "bathurst"        , 19_000 },
        { "brands_hatch"    , 19_000 },
        { "cota"            , 29_000 },
        { "donington"       , 19_000 },
        { "hungaroring"     , 24_000 },
        { "imola"           , 38_000 },
        { "indianapolis"    , 44_000 },
        { "kyalami"         , 18_000 },
        { "laguna_seca"     , 21_000 },
        { "misano"          , 28_000 },
        { "monza"           , 31_000 },
        { "nurburgring"     , 25_000 },
        { "nurburgring_24h" , 25_000 },
        { "oulton_park"     , 14_000 },
        { "paul_ricard"     , 27_000 },
        { "silverstone"     , 23_000 },
        { "snetterton"      , 19_000 },
        { "spa"             , 57_000 },
        { "suzuka"          , 27_000 },
        { "valencia"        , 27_000 },
        { "watkins_glen"    , 27_000 },
        { "zandvoort"       , 19_000 },
        { "zolder"          , 30_000 },
        { "mount_panorama"  , 25_000 },
        { "red_bull_ring"   , 20_000 }
    };

    public static PitStop GetPitStop(List<PointF> track)
    {
        var name = ACCSharedMemory.Instance.PageFileStatic.Track.ToLower();
        float ms = TrackPitLineTimeMs.GetValueOrDefault(name, 0.0f);
        return ComputePitStop(track, DefaultPitTimeMs + ms);
    }

    public static PitStop GetPitStopWithDamage(List<PointF> track)
    {
        var p = ACCSharedMemory.Instance.PageFilePhysics;
        var result = RaceElement.Data.ACC.Cars.Damage.GetTotalRepairTime(p);

        if (result == 0)
        {
            return null;
        }

        var name = ACCSharedMemory.Instance.PageFileStatic.Track.ToLower();
        var ms = TrackPitLineTimeMs.GetValueOrDefault(name, 0.0f);

        result = result * 1000 + DefaultPitTimeMs + ms;
        var pitstop = ComputePitStop(track, result);

        if (pitstop != null)
        {
            pitstop.Damage = true;
        }

        return pitstop;
    }

    private static PitStop ComputePitStop(List<PointF> track, float time)
    {
        var pageFileGraphic = ACCSharedMemory.Instance.PageFileGraphic;
        var estimatedLapTime = pageFileGraphic.EstimatedLapTimeMillis;
        var currenTime = pageFileGraphic.CurrentTimeMs;

        if (estimatedLapTime == Int32.MaxValue)
        {
            return null;
        }

        var diffTime = currenTime - (time % estimatedLapTime);
        diffTime = diffTime >= 0 ? diffTime : estimatedLapTime + diffTime;

        var delta = diffTime / estimatedLapTime;
        var trackPos = (int)(delta * track.Count);

        var result = new PitStop
        {
            Position = track[trackPos],
            Laps = (int)(time / pageFileGraphic.BestTimeMs)
        };

        return result;
    }
}
