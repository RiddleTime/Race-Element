using RaceElement.HUD.Overlay.OverlayUtil;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Common.Overlays.Flight.ControlsTrace;

internal sealed class ControlsGraph : IDisposable
{
    private readonly int _x, _y;
    private readonly int _width, _height;
    private readonly ControlsTraceConfiguration _config;

    private readonly CachedBitmap _cachedBackground;
    private readonly Pen _aileronPen;
    private readonly Pen _elevatorPen;
    private readonly Pen _rudderPen;


    // Reusable buffers to avoid per-frame allocations
    private readonly List<double> _aileronBuffer = [];
    private readonly List<double> _elevatorBuffer = [];
    private readonly List<double> _rudderBuffer = [];
    private readonly List<Point> _pointsBuffer = [];

    public ControlsGraph(int x, int y, int width, int height, ControlsTraceConfiguration config)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _config = config;

        _aileronPen = new Pen(Color.FromArgb(_config.Colors.AileronOpacity, _config.Colors.AileronColor), _config.Chart.LineThickness);
        _elevatorPen = new Pen(Color.FromArgb(_config.Colors.ElevatorOpacity, _config.Colors.ElevatorColor), _config.Chart.LineThickness);
        _rudderPen = new Pen(Color.FromArgb(_config.Colors.RudderOpacity, _config.Colors.RudderColor), _config.Chart.LineThickness);

        _cachedBackground = new CachedBitmap(_width + 1, _height + 1, g =>
        {
            if (config.Chart.GridLines)
            {
                using Pen linePen = new(new SolidBrush(Color.FromArgb(90, Color.White)), 1);
                for (int i = 1; i <= 9; i++)
                    g.DrawLine(linePen, new Point(0, i * _height / 10), new Point(_width, i * _height / 10));
            }

            Rectangle graphRect = new(_x, _y, _width, _height);
            using LinearGradientBrush gradientBrush = new(graphRect, Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);
            g.FillRoundedRectangle(gradientBrush, graphRect, 3);
            g.DrawRoundedRectangle(new Pen(Color.FromArgb(196, Color.Black)), graphRect, 3);
        });
    }

    private int GetRelativeNodeY(double value)
    {
        double range = 100 - 0;
        double percentage = 1d - (value - 0) / range;
        return (int)(percentage * (_height - _height / 5))
                + _height / 10;
    }

    public void Draw(Graphics g, ConcurrentQueue<ControlsData> data)
    {
        _cachedBackground?.Draw(g);

        g.SmoothingMode = SmoothingMode.HighQuality;

        _aileronBuffer.Clear();
        _elevatorBuffer.Clear();
        _rudderBuffer.Clear();

        foreach (var item in data)
        {
            _aileronBuffer.Add(item.Aileron);
            _elevatorBuffer.Add(item.Elevator);
            _rudderBuffer.Add(item.Rudder);
        }

        DrawData(g, _aileronBuffer, _aileronPen);
        DrawData(g, _elevatorBuffer, _elevatorPen);
        DrawData(g, _rudderBuffer, _rudderPen);
    }

    private void DrawData(Graphics g, List<double> data, Pen pen)
    {
        if (data.Count > 0)
        {
            _pointsBuffer.Clear();

            int dataLength = data.Count;
            for (int i = dataLength - 1; i >= 0; i--)
            {
                int x = _x + i * (_width / dataLength);
                int y = _y + GetRelativeNodeY(data[i]);

                if (x < _x)
                    break;

                _pointsBuffer.Add(new Point(x, y));
            }

            if (_pointsBuffer.Count > 0)
            {
                using GraphicsPath path = new();
                path.AddLines(_pointsBuffer.ToArray());
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
        _aileronPen?.Dispose();
        _elevatorPen?.Dispose();
        _rudderPen?.Dispose();
    }
}
