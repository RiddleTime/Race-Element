using RaceElement.HUD.Overlay.OverlayUtil;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.Common.Overlays.Driving.GForceTrace;

internal sealed class GForceGraph : IDisposable
{
    private readonly int _x, _y;
    private readonly int _width, _height;
    private readonly GForceDataJob _dataJob;

    private readonly CachedBitmap _cachedBackground;
    private readonly Pen _longPen;
    private readonly Pen _latPen;

    private List<int> _drawingData = [];

    public GForceGraph(int x, int y, int width, int height, GForceDataJob dataJob, GForceTraceConfiguration config)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _dataJob = dataJob;

        _longPen = new Pen(Color.LightGray, config.Chart.LineThickness);
        _latPen = new Pen(Color.Yellow, config.Chart.LineThickness);

        _cachedBackground = new CachedBitmap(_width + 1, _height + 1, g =>
        {
            if (config.Chart.GridLines)
            {
                using SolidBrush lineBrush = new(Color.FromArgb(90, Color.White));
                using Pen linePen = new(lineBrush, 1);
                for (int i = 1; i <= 9; i++)
                    g.DrawLine(linePen, new Point(0, i * _height / 10), new Point(_width, i * _height / 10));
            }

            Rectangle graphRect = new(_x, _y, _width, _height);
            using LinearGradientBrush gradientBrush = new(graphRect, Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);
            g.FillRoundedRectangle(gradientBrush, graphRect, 3);
            using Pen outlinePen = new(Color.FromArgb(196, Color.Black));
            g.DrawRoundedRectangle(outlinePen, graphRect, 3);
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

        if (_dataJob != null)
        {
            lock (_dataJob._lock) _drawingData = new(_dataJob.Longitudinal);
            DrawData(g, _drawingData, _longPen);

            lock (_dataJob._lock) _drawingData = new(_dataJob.Lateral);
            DrawData(g, _drawingData, _latPen);
        }
    }

    private void DrawData(Graphics g, List<int> data, Pen pen)
    {
        if (data.Count > 0)
        {
            List<Point> points = [];
            ReadOnlySpan<int> spanData = CollectionsMarshal.AsSpan(data);
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

    protected void Dispose(bool disposing)
    {
        _cachedBackground?.Dispose();
        _longPen?.Dispose();
        _latPen?.Dispose();
    }
}
