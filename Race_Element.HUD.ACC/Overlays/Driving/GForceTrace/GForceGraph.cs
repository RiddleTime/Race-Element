using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

internal class GForceGraph : IDisposable
{
    private readonly int _x, _y;
    private readonly int _width, _height;
    private readonly GForceDataJob _collector;

    private readonly CachedBitmap _cachedBackground;
    private readonly Pen _longPen;
    private readonly Pen _latPen;

    public GForceGraph(int x, int y, int width, int height, GForceDataJob collector, GForceTraceConfiguration config)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _collector = collector;

        _longPen = new Pen(Color.LightGray, config.Chart.LineThickness);
        _latPen = new Pen(Color.Yellow, config.Chart.LineThickness);

        _cachedBackground = new CachedBitmap(_width + 1, _height + 1, g =>
        {
            if (config.Chart.GridLines)
            {
                using Pen linePen = new(new SolidBrush(Color.FromArgb(90, Color.White)), 1);
                for (int i = 1; i <= 9; i++)
                    g.DrawLine(linePen, new Point(0, i * _height / 10), new Point(_width, i * _height / 10));
            }

            Rectangle graphRect = new(_x, _y, _width, _height);
            LinearGradientBrush gradientBrush = new(graphRect, Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);
            g.FillRoundedRectangle(gradientBrush, graphRect, 3);
            g.DrawRoundedRectangle(new Pen(Color.FromArgb(196, Color.Black)), graphRect, 3);
        });
    }

    private int GetRelativeNodeY(int value)
    {
        double range = 100 - 0;
        double percentage = 1d - (value - 0) / range;
        return (int)(percentage * (_height - _height / 5))
                + _height / 10;
    }

    public void Draw(Graphics g)
    {
        _cachedBackground?.Draw(g);

        g.SmoothingMode = SmoothingMode.HighQuality;

        if (_collector != null)
        {
            LinkedList<int> data;

            lock (_collector.Longitudinal)
                data = new(_collector.Longitudinal);
            DrawData(g, data, _longPen);

            lock (_collector.Lateral)
                data = new(_collector.Lateral);
            DrawData(g, data, _latPen);
        }
    }

    private void DrawData(Graphics g, LinkedList<int> Data, Pen pen)
    {
        if (Data.Count > 0)
        {
            List<Point> points = [];
            for (int i = 0; i < Data.Count - 1; i++)
            {
                int x = _x + _width - i * (_width / Data.Count);
                int y = _y + GetRelativeNodeY(Data.ElementAt(i));

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
        _longPen?.Dispose();
        _latPen?.Dispose();
    }
}
