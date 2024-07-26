using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.ACC.Overlays.Driving.OversteerTrace;

internal class OversteerGraph : IDisposable
{
    private readonly int _x, _y;
    private readonly int _width, _height;
    private readonly OversteerDataJob _dataJob;

    private readonly CachedBitmap _cachedBackground;
    private readonly Pen _penOversteer;
    private readonly Pen _penUndersteer;

    public OversteerGraph(int x, int y, int width, int height, OversteerDataJob dataJob, OversteerTraceConfiguration config)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _dataJob = dataJob;

        _penOversteer = new Pen(Color.OrangeRed, config.Chart.LineThickness);
        _penUndersteer = new Pen(Color.DeepSkyBlue, config.Chart.LineThickness);

        _cachedBackground = new CachedBitmap(_width + 1, _height + 1, g =>
        {
            if (config.Chart.GridLines)
            {
                using SolidBrush lineBrush = new(Color.FromArgb(90, Color.White));
                using Pen linePen = new(lineBrush, 1);
                for (int i = 1; i <= 9; i++)
                    g.DrawLine(linePen, new Point(0, i * _height / 10), new Point(_width, i * _height / 10));
            }

            Rectangle graphRect = new(0, 0, _width, _height);
            using LinearGradientBrush gradientBrush = new(graphRect, Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);
            g.FillRoundedRectangle(gradientBrush, graphRect, 3);
            using Pen outlinePen = new(Color.FromArgb(196, Color.Black));
            g.DrawRoundedRectangle(outlinePen, graphRect, 3);
        });
    }

    private int GetRelativeNodeY(float value)
    {
        double range = _dataJob.MaxSlipAngle - 0;
        double percentage = 1d - (value - 0) / range;
        return (int)(percentage * (_height - _height / 5))
                + _height / 10;
    }

    public void Draw(Graphics g)
    {
        _cachedBackground?.Draw(g, _x, _y, _width, _height);

        g.SmoothingMode = SmoothingMode.HighQuality;

        if (_dataJob != null)
        {
            List<float> data;

            lock (_dataJob.Oversteer) data = new(_dataJob.Oversteer);
            DrawData(g, data, _penOversteer);

            lock (_dataJob.Understeer) data = new(_dataJob.Understeer);
            DrawData(g, data, _penUndersteer);
        }
    }

    private void DrawData(Graphics g, List<float> data, Pen pen)
    {
        if (data.Count > 0)
        {
            List<Point> points = [];
            var spanData = CollectionsMarshal.AsSpan(data);
            for (int i = 0; i < spanData.Length - 1; i++)
            {
                int x = _x + _width - i * (_width / spanData.Length);

                int y = _y + GetRelativeNodeY(spanData[i]);

                if (x < _x)
                    break;

                points.Add(new Point(x, y));
            }

            if (points.Count > 0)
            {
                using GraphicsPath path = new();
                path.AddLines(points.ToArray());
                g.DrawPath(pen, path);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _cachedBackground?.Dispose();
        _penOversteer?.Dispose();
        _penUndersteer?.Dispose();
    }
}
