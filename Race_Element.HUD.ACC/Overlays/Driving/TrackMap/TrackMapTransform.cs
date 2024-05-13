
using System;
using System.Drawing;

using System.Collections.Generic;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public struct BoundingBox
{
    public float Left, Right;
    public float Bottom, Top;
}

public class TrackMapTransform
{
    public static BoundingBox GetBoundingBox(List<PointF> positions)
    {
        BoundingBox result = new();

        if (positions.Count > 0)
        {
            result.Right = positions[0].X;
            result.Left = positions[0].X;

            result.Top = positions[0].Y;
            result.Bottom = positions[0].Y;
        }

        foreach (var it in positions)
        {
            result.Right = Math.Max(result.Right, it.X);
            result.Left = Math.Min(result.Left, it.X);

            result.Top = Math.Max(result.Top, it.Y);
            result.Bottom = Math.Min(result.Bottom, it.Y);
        }

        return result;
    }

    public static PointF ScaleAndRotate(PointF point, BoundingBox boundaries, float scale, float rotation)
    {
        PointF pos = new();
        var rot = Double.DegreesToRadians(rotation);

        var centerX = (boundaries.Right + boundaries.Left) * 0.5f;
        var centerY = (boundaries.Top + boundaries.Bottom) * 0.5f;

        pos.X = (point.X - centerX) * scale;
        pos.Y = (point.Y - centerY) * scale;

        var x = pos.X * Math.Cos(rot) - pos.Y * Math.Sin(rot);
        var y = pos.X * Math.Sin(rot) + pos.Y * Math.Cos(rot);

        pos.X = (float)x;
        pos.Y = (float)y;

        return pos;
    }

    public static List<PointF> ScaleAndRotate(List<PointF> positions, BoundingBox boundaries, float scale, float rotation)
    {
        List<PointF> result = new();
        foreach (var it in positions)
        {
            var pos = ScaleAndRotate(it, boundaries, scale, rotation);
            result.Add(pos);
        }

        return result;
    }
}
