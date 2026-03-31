using RaceElement.HUD.Overlay.OverlayUtil;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Common.Overlays.Driving.InputTrace;

internal sealed class InputGraph : IDisposable
{
    private readonly int _x, _y;
    private readonly int _width, _height;
    private readonly InputTraceConfiguration _config;

    private readonly CachedBitmap _cachedBackground;
    private readonly Pen _throttlePen;
    private readonly Pen _brakePen;
    private readonly Pen _clutchPen;
    private readonly Pen _steeringPen;
    private readonly Pen _tractionControlPen;
    private readonly Pen _absPen;


    // Reusable buffers to avoid per-frame allocations
    private readonly List<int> _throttleBuffer = [];
    private readonly List<int> _brakeBuffer = [];
    private readonly List<int> _clutchBuffer = [];
    private readonly List<int> _steeringBuffer = [];
    private readonly List<bool> _tractionControlBuffer = [];
    private readonly List<bool> _absBuffer = [];
    private readonly List<Point> _pointsBuffer = [];

    public InputGraph(int x, int y, int width, int height, InputTraceConfiguration config)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _config = config;

        _throttlePen = new Pen(Color.FromArgb(_config.Colors.ThrottleOpacity, _config.Colors.ThrottleColor), _config.Chart.LineThickness);
        _brakePen = new Pen(Color.FromArgb(_config.Colors.BrakeOpacity, _config.Colors.BrakeColor), _config.Chart.LineThickness);
        _clutchPen = new Pen(Color.FromArgb(_config.Colors.ClutchOpacity, _config.Colors.ClutchColor), _config.Chart.LineThickness);
        _steeringPen = new Pen(Color.FromArgb(_config.Colors.SteeringOpacity, _config.Colors.SteeringColor), _config.Chart.LineThickness);
        _tractionControlPen = new Pen(Color.FromArgb(_config.TractionControl.TractionControlOpacity, _config.TractionControl.TractionControlColor), 1);
        _absPen = new Pen(Color.FromArgb(_config.Abs.AbsOpacity, _config.Abs.AbsColor), 1);

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

    private int GetRelativeNodeY(int value)
    {
        double range = 100 - 0;
        double percentage = 1d - (value - 0) / range;
        return (int)(percentage * (_height - _height / 5))
                + _height / 10;
    }

    public void Draw(Graphics g, ConcurrentQueue<InputsData> data)
    {
        _cachedBackground?.Draw(g);

        g.SmoothingMode = SmoothingMode.HighQuality;

        _throttleBuffer.Clear();
        _brakeBuffer.Clear();
        _clutchBuffer.Clear();
        _steeringBuffer.Clear();
        _tractionControlBuffer.Clear();
        _absBuffer.Clear();

        foreach (var item in data)
        {
            _throttleBuffer.Add(item.Throttle);
            _brakeBuffer.Add(item.Brake);
            _clutchBuffer.Add(item.Clutch);
            _steeringBuffer.Add(item.Steering);
            _tractionControlBuffer.Add(item.TractionControlActivation);
            _absBuffer.Add(item.AbsActivation);
        }

        if (_config.Chart.ClutchInput)
        {
            DrawData(g, _clutchBuffer, _clutchPen);
        }

        if (_config.Chart.SteeringInput)
        {
            DrawData(g, _steeringBuffer, _steeringPen);
        }

        DrawData(g, _throttleBuffer, _throttlePen);
        if (_config.TractionControl.TractionControl)
        {
            DrawData(g, _tractionControlBuffer, _throttleBuffer, _tractionControlPen);
        }

        DrawData(g, _brakeBuffer, _brakePen);
        if (_config.Abs.Abs)
        {
            DrawData(g, _absBuffer, _brakeBuffer, _absPen);
        }
    }

    private void DrawData(Graphics g, List<bool> bData, List<int> iData, Pen pen)
    {
        if (bData.Count > 0 && iData.Count > 0 && bData.Count == iData.Count)
        {
            int dataLength = bData.Count;
            for (int i = dataLength - 1; i >= 0; i--)
            {
                if (bData[i])
                {
                    int x = _x + i * (_width / dataLength);
                    int y = _y + GetRelativeNodeY(iData[i]);

                    g.DrawLine(pen, new Point(x, y - 1), new Point(x, 2));
                }
            }
        }
    }

    private void DrawData(Graphics g, List<int> data, Pen pen)
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
        _throttlePen?.Dispose();
        _brakePen?.Dispose();
        _clutchPen?.Dispose();
        _steeringPen?.Dispose();
        _tractionControlPen?.Dispose();
        _absPen?.Dispose();
    }
}
