using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.OversteerTrace;

internal class OversteerGraph : IDisposable
{
    private readonly int _x, _y;
    private readonly int _width, _height;
    private readonly OversteerDataCollector _collector;

    private readonly CachedBitmap _cachedBackground;
    private readonly Pen _penOversteer;
    private readonly Pen _penUndersteer;

    public OversteerGraph(int x, int y, int width, int height, OversteerDataCollector collector, OversteerTraceConfiguration config)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _collector = collector;

        _penOversteer = new Pen(Color.OrangeRed, config.Chart.LineThickness);
        _penUndersteer = new Pen(Color.DeepSkyBlue, config.Chart.LineThickness);

        _cachedBackground = new CachedBitmap(_width + 1, _height + 1, g =>
        {
            Rectangle graphRect = new(0, 0, _width, _height);
            LinearGradientBrush gradientBrush = new(graphRect, Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);
            g.FillRoundedRectangle(gradientBrush, graphRect, 3);
            g.DrawRoundedRectangle(new Pen(Color.FromArgb(196, Color.Black)), graphRect, 3);
        });
    }

    private int GetRelativeNodeY(float value)
    {
        double range = _collector.MaxSlipAngle - 0;
        double percentage = 1d - (value - 0) / range;
        return (int)(percentage * (_height - _height / 5))
                + _height / 10;
    }

    public void Draw(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.HighQuality;

        _cachedBackground?.Draw(g, _x, _y, _width, _height);

        DrawData(g, _collector.OversteerData, _penOversteer);
        DrawData(g, _collector.UndersteerData, _penUndersteer);
    }

    private void DrawData(Graphics g, LinkedList<float> Data, Pen pen)
    {
        if (Data.Count > 0)
        {
            List<Point> points = [];
            lock (Data)
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
        _penOversteer?.Dispose();
        _penUndersteer?.Dispose();
    }
}
