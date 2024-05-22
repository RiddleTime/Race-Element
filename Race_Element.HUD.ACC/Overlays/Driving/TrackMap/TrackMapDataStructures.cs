using System;
using System.Drawing;
using System.Collections.Generic;

using RaceElement.Broadcast;
namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public class BoundingBox
{
    public float Left, Right;
    public float Bottom, Top;
}

public class CarOnTrack
{
    public string RaceNumber;

    public CarLocationEnum Location;
    public TrackPoint Pos;

    public bool IsValidForBest;
    public bool IsValid;

    public float Spline;
    public int Position;
    public int Kmh;

    public int Delta;
    public int Laps;
    public int Id;
}

public class CarRenderData
{
    public readonly List<CarOnTrack> Cars = [];
    public CarOnTrack Player;
}

public class TrackMapCache
{
    public Bitmap OthersLappedPlayer;
    public Bitmap PlayerLapperOthers;

    public Bitmap CarDefault;
    public Bitmap CarPlayer;

    public Bitmap PitStopWithDamage;
    public Bitmap PitStop;

    public Bitmap ValidForBest;
    public Bitmap Leader;

    public Bitmap YellowFlag;
    public Bitmap Map;
}

public class TrackPoint
{
    private float _x, _y, _spline;

    public TrackPoint()
    {
        _x = 0;
        _y = 0;
        _spline = 0;
    }

    public TrackPoint(TrackPoint other)
    {
        _x = other._x;
        _y = other._y;
        _spline = other._spline;
    }

    public float X
    {
        get => _x;
        set => _x = value;
    }

    public float Y
    {
        get => _y;
        set => _y = value;
    }

    public float Spline
    {
        get => _spline;
        set => _spline = value;
    }

    public PointF ToPointF()
    {
        return new PointF(_x, _y);
    }
}

public class TrackPointSplineComparator(float epsilon = 0.0005f) : IComparer<TrackPoint>
{
    public int Compare(TrackPoint x, TrackPoint y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        var diff = x.Spline - y.Spline;
        if (Math.Abs(diff) < epsilon)
        {
            return 0;
        }

        return diff < 0 ? -1 : 1;
    }
}

public class PitStop
{
    public TrackPoint Position;
    public bool Damage;
    public int Laps;
}

public class TrackInfo(float pitLaneTimeMs, float lengthMeters, float scale)
{
    public readonly float PitLaneTimeMs = pitLaneTimeMs;
    public readonly float LengthMeters = lengthMeters;
    public readonly float Scale = scale;

    public static readonly Dictionary<string, TrackInfo> Data = new()
    {
        { "barcelona"       , new TrackInfo(30_000,  4655, 0.310f) },
        { "brands_hatch"    , new TrackInfo(19_000,  3908, 0.370f) },
        { "cota"            , new TrackInfo(29_000,  5513, 0.200f) },
        { "donington"       , new TrackInfo(19_000,  4020, 0.270f) },
        { "hungaroring"     , new TrackInfo(24_000,  4381, 0.230f) },
        { "imola"           , new TrackInfo(38_000,  4959, 0.200f) },
        { "indianapolis"    , new TrackInfo(44_000,  4167, 0.250f) },
        { "kyalami"         , new TrackInfo(18_000,  4522, 0.200f) },
        { "laguna_seca"     , new TrackInfo(21_000,  3602, 0.360f) },
        { "misano"          , new TrackInfo(28_000,  4226, 0.310f) },
        { "monza"           , new TrackInfo(31_000,  5793, 0.160f) },
        { "nurburgring"     , new TrackInfo(25_000,  5137, 0.240f) },
        { "nurburgring_24h" , new TrackInfo(25_000, 25300, 0.061f) },
        { "oulton_park"     , new TrackInfo(14_000,  4307, 0.230f) },
        { "paul_ricard"     , new TrackInfo(27_000,  5770, 0.160f) },
        { "silverstone"     , new TrackInfo(23_000,  5891, 0.210f) },
        { "snetterton"      , new TrackInfo(19_000,  4779, 0.300f) },
        { "spa"             , new TrackInfo(57_000,  7004, 0.190f) },
        { "suzuka"          , new TrackInfo(27_000,  5807, 0.180f) },
        { "valencia"        , new TrackInfo(27_000,  4005, 0.370f) },
        { "watkins_glen"    , new TrackInfo(27_000,  5552, 0.210f) },
        { "zandvoort"       , new TrackInfo(19_000,  4252, 0.270f) },
        { "zolder"          , new TrackInfo(30_000,  4011, 0.280f) },
        { "mount_panorama"  , new TrackInfo(25_000,  6213, 0.170f) },
        { "red_bull_ring"   , new TrackInfo(20_000,  4318, 0.290f) }
    };
}
