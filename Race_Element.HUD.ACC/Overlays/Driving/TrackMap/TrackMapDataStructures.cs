using System;
using System.Drawing;
using System.Collections.Generic;

using RaceElement.Broadcast;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public class BoundingBox
{
    public float Left, Right;
    public float Bottom, Top;
}

public class CarOnTrack
{
    public string RaceNumber;
    public string RacePosition;
    public CarClasses CarClass;

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
    public Bitmap CarPlayer;
    public Bitmap Map;

    public Bitmap PitStopWithDamage;
    public Bitmap PitStop;
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

public class TrackInfo(float pitLaneTimeMs, float scale, float sector1End, float sector2End)
{
    public readonly float PitLaneTimeMs = pitLaneTimeMs;
    public readonly float Scale = scale;

    public readonly float Sector1End = sector1End;
    public readonly float Sector2End = sector2End;

    public static readonly Dictionary<string, TrackInfo> Data = new()
    {
        { "barcelona"       , new TrackInfo(30_000, 0.310f, 0.348f, 0.730f) },
        { "brands_hatch"    , new TrackInfo(19_000, 0.370f, 0.287f, 0.563f) },
        { "cota"            , new TrackInfo(29_000, 0.200f, 0.236f, 0.647f) },
        { "donington"       , new TrackInfo(19_000, 0.270f, 0.233f, 0.668f) },
        { "hungaroring"     , new TrackInfo(24_000, 0.230f, 0.397f, 0.748f) },
        { "imola"           , new TrackInfo(38_000, 0.200f, 0.247f, 0.552f) },
        { "indianapolis"    , new TrackInfo(44_000, 0.250f, 0.390f, 0.719f) },
        { "kyalami"         , new TrackInfo(18_000, 0.200f, 0.316f, 0.710f) },
        { "laguna_seca"     , new TrackInfo(21_000, 0.360f,  0.25f,  0.63f) },
        { "misano"          , new TrackInfo(28_000, 0.310f, 0.222f, 0.621f) },
        { "monza"           , new TrackInfo(31_000, 0.160f, 0.334f, 0.661f) },
        { "nurburgring"     , new TrackInfo(25_000, 0.240f, 0.451f, 0.858f) },
        { "nurburgring_24h" , new TrackInfo(25_000, 0.061f,   0.0f,   0.0f) },
        { "oulton_park"     , new TrackInfo(14_000, 0.230f, 0.264f, 0.693f) },
        { "paul_ricard"     , new TrackInfo(27_000, 0.160f, 0.264f, 0.591f) },
        { "silverstone"     , new TrackInfo(23_000, 0.210f, 0.315f, 0.707f) },
        { "snetterton"      , new TrackInfo(19_000, 0.300f, 0.321f, 0.678f) },
        { "spa"             , new TrackInfo(57_000, 0.190f, 0.330f, 0.716f) },
        { "suzuka"          , new TrackInfo(27_000, 0.180f, 0.324f, 0.757f) },
        { "valencia"        , new TrackInfo(27_000, 0.370f, 0.379f, 0.725f) },
        { "watkins_glen"    , new TrackInfo(27_000, 0.210f, 0.319f, 0.632f) },
        { "zandvoort"       , new TrackInfo(19_000, 0.270f, 0.318f, 0.654f) },
        { "zolder"          , new TrackInfo(30_000, 0.280f, 0.363f, 0.684f) },
        { "mount_panorama"  , new TrackInfo(25_000, 0.170f, 0.334f, 0.661f) },
        { "red_bull_ring"   , new TrackInfo(20_000, 0.290f, 0.286f, 0.686f) }
    };
}
