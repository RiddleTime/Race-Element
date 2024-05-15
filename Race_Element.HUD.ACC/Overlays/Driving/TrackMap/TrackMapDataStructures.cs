using System;
using System.Drawing;
using System.Collections.Generic;

using RaceElement.Broadcast;
namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public struct BoundingBox
{
    public float Left, Right;
    public float Bottom, Top;
}

class CarOnTrack
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

class CarRenderData
{
    public List<CarOnTrack> Cars = [];
    public CarOnTrack Player;
}

class TrackMapCache
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
    private float _deltaTime, _kmh;
    private float _accX, _accY, _accZ;

    public TrackPoint()
    {
        _x = 0;
        _y = 0;
        _spline = 0;

        _kmh = 0;
        _deltaTime = 0;

        _accX = 0;
        _accY = 0;
        _accZ = 0;
    }

    public TrackPoint(TrackPoint other)
    {
        _x = other._x;
        _y = other._y;
        _spline = other._spline;

        _kmh = other._kmh;
        _deltaTime = other._deltaTime;

        _accX = other._accX;
        _accY = other._accY;
        _accZ = other._accZ;
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

    public float DeltaTime
    {
        get => _deltaTime;
        set => _deltaTime = value;
    }

    public float Kmh
    {
        get => _kmh;
        set => _kmh = value;
    }

    public float AccX
    {
        get => _accX;
        set => _accX = value;
    }

    public float AccY
    {
        get => _accY;
        set => _accY = value;
    }

    public float AccZ
    {
        get => _accZ;
        set => _accZ = value;
    }

    public PointF ToPointF()
    {
        return new PointF(_x, _y);
    }
}

public class TrackPointSplineComparator : IComparer<TrackPoint>
{
    private float _epsilon;

    public TrackPointSplineComparator()
    {
        _epsilon = 0.0005f;
    }

    public TrackPointSplineComparator(float epsilon)
    {
        _epsilon = epsilon;
    }

    public int Compare(TrackPoint x, TrackPoint y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        var diff = x.Spline - y.Spline;
        if (Math.Abs(diff) < _epsilon)
        {
            return 0;
        }

        return diff > 0 ? 1 : -1;
    }
}

public class PitStop
{
    public TrackPoint Position;
    public bool Damage;
    public int Laps;
}

public class TrackInfo
{
    public TrackInfo(float pitLaneTimeMs, float lengthMeters)
    {
        PitLaneTimeMs = pitLaneTimeMs;
        LengthMeters = lengthMeters;
    }

    public float PitLaneTimeMs;
    public float LengthMeters;

    public static readonly Dictionary<string, TrackInfo> Data = new()
    {
        { "barcelona"       , new TrackInfo(30_000,  4655) },
        { "brands_hatch"    , new TrackInfo(19_000,  3908) },
        { "cota"            , new TrackInfo(29_000,  5513) },
        { "donington"       , new TrackInfo(19_000,  4020) },
        { "hungaroring"     , new TrackInfo(24_000,  4381) },
        { "imola"           , new TrackInfo(38_000,  4959) },
        { "indianapolis"    , new TrackInfo(44_000,  4167) },
        { "kyalami"         , new TrackInfo(18_000,  4522) },
        { "laguna_seca"     , new TrackInfo(21_000,  3602) },
        { "misano"          , new TrackInfo(28_000,  4226) },
        { "monza"           , new TrackInfo(31_000,  5793) },
        { "nurburgring"     , new TrackInfo(25_000,  5137) },
        { "nurburgring_24h" , new TrackInfo(25_000, 25300) },
        { "oulton_park"     , new TrackInfo(14_000,  4307) },
        { "paul_ricard"     , new TrackInfo(27_000,  5770) },
        { "silverstone"     , new TrackInfo(23_000,  5891) },
        { "snetterton"      , new TrackInfo(19_000,  4779) },
        { "spa"             , new TrackInfo(57_000,  7004) },
        { "suzuka"          , new TrackInfo(27_000,  5807) },
        { "valencia"        , new TrackInfo(27_000,  4005) },
        { "watkins_glen"    , new TrackInfo(27_000,  5552) },
        { "zandvoort"       , new TrackInfo(19_000,  4252) },
        { "zolder"          , new TrackInfo(30_000,  4011) },
        { "mount_panorama"  , new TrackInfo(25_000,  6213) },
        { "red_bull_ring"   , new TrackInfo(20_000,  4318) }
    };
}
