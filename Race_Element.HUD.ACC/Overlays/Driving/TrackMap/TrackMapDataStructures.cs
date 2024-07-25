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
