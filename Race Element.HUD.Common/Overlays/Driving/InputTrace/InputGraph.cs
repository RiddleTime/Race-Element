using RaceElement.HUD.Overlay.OverlayUtil;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.Common.Overlays.Driving.InputTrace;

internal class InputGraph : IDisposable
{
    private readonly int _x, _y;
    private readonly int _width, _height;
    private readonly InputTraceConfiguration _config;
    private readonly InputDataJob _dataJob;

    private readonly CachedBitmap _cachedBackground;
    private readonly Pen _throttlePen;
    private readonly Pen _brakePen;
    private readonly Pen _steeringPen;

    public InputGraph(int x, int y, int width, int height, InputDataJob dataJob, InputTraceConfiguration config)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _dataJob = dataJob;
        _config = config;

        _throttlePen = new Pen(Color.ForestGreen, _config.Chart.LineThickness);
        _brakePen = new Pen(Color.Red, _config.Chart.LineThickness);
        _steeringPen = new Pen(Color.FromArgb(190, Color.White), _config.Chart.LineThickness);

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

        if (_dataJob != null)
        {
            List<int> data;

            if (_config.Chart.SteeringInput)
            {
                lock (_dataJob.Steering) data = new(_dataJob.Steering);
                DrawData(g, data, _steeringPen);
            }

            lock (_dataJob.Throttle) data = new(_dataJob.Throttle);
            DrawData(g, data, _throttlePen);

            lock (_dataJob.Brake) data = new(_dataJob.Brake);
            DrawData(g, data, _brakePen);
        }
    }

    private void DrawData(Graphics g, List<int> data, Pen pen)
    {
        if (data.Count > 0)
        {
            List<Point> points = [];
            var spanData = CollectionsMarshal.AsSpan<int>(data);
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
        _throttlePen?.Dispose();
        _brakePen?.Dispose();
        _steeringPen?.Dispose();
    }
}
