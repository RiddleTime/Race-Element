using RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.OverlayInputTrace;

internal class GForceGraph : IDisposable
{
    private readonly int _x, _y;
    private readonly int _width, _height;
    private readonly GForceTraceConfiguration _config;
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
        _config = config;

        _longPen = new Pen(Color.LightGray, _config.Chart.LineThickness);
        _latPen = new Pen(Color.Yellow, _config.Chart.LineThickness);

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
            DrawData(g, _collector.Longitudinal, _longPen);
            DrawData(g, _collector.Lateral, _latPen);
        }
    }

    private void DrawData(Graphics g, LinkedList<int> Data, Pen pen)
    {
        if (Data.Count > 0)
        {
            List<Point> points = [];
            lock (Data)
                for (int i = 0; i < Data.Count - 1; i++)
                {
                    int x = _x + _width - i * (_width / Data.Count);
                    lock (Data)
                    {
                        int y = _y + GetRelativeNodeY(Data.ElementAt(i));

                        if (x < _x)
                            break;

                        points.Add(new Point(x, y));
                    }
                }

            if (points.Count > 0)
            {
                GraphicsPath path = new();
                path.AddLines(points.ToArray());
                g.DrawPath(pen, path);
                path?.Dispose();
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
