using RaceElement.HUD.Overlay.OverlayUtil;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.Common.Overlays.Driving.InputTrace;

internal sealed class InputGraph : IDisposable
{
    private readonly int _x, _y;
    private readonly int _width, _height;
    private readonly InputTraceConfiguration _config;

    private readonly CachedBitmap _cachedBackground;
    private readonly Pen _throttlePen;
    private readonly Pen _brakePen;
    private readonly Pen _steeringPen;
    private readonly Pen _tractionControlPen;
    private readonly Pen _absPen;

    public InputGraph(int x, int y, int width, int height, InputTraceConfiguration config)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _config = config;

        _throttlePen = new Pen(Color.FromArgb(_config.Colors.ThrottleOpacity, _config.Colors.ThrottleColor), _config.Chart.LineThickness);
        _brakePen = new Pen(Color.FromArgb(_config.Colors.BrakeOpacity, _config.Colors.BrakeColor), _config.Chart.LineThickness);
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

    private List<int> _iData = [];
    private List<bool> _bData = [];
    public void Draw(Graphics g, ConcurrentQueue<InputsData> data)
    {
        _cachedBackground?.Draw(g);

        g.SmoothingMode = SmoothingMode.HighQuality;

        if (_config.Chart.SteeringInput)
        {
            _iData = new(data.Select(x => x.Steering));
            DrawData(g, _iData, _steeringPen);
        }

        _iData = new(data.Select(x => x.Throttle));
        DrawData(g, _iData, _throttlePen);
        if (_config.TractionControl.TractionControl)
        {
            _bData = new(data.Select(x => x.TractionControlActivation));
            DrawData(g, _bData, _iData, _tractionControlPen);
        }


        _iData = new(data.Select(x => x.Brake));
        DrawData(g, _iData, _brakePen);
        if (_config.Abs.Abs)
        {
            _bData = new(data.Select(x => x.AbsActivation));
            DrawData(g, _bData, _iData, _absPen);
        }
    }

    private void DrawData(Graphics g, List<bool> bData, List<int> iData, Pen pen)
    {
        if (bData.Count > 0 && iData.Count > 0 && bData.Count == iData.Count)
        {
            var bSpan = CollectionsMarshal.AsSpan<bool>(bData);
            var iSpan = CollectionsMarshal.AsSpan<int>(iData);
            for (int i = bSpan.Length - 1; i >= 0; i--)
            {
                if (bSpan[i])
                {
                    int x = _x + i* (_width / bSpan.Length);
                    int y = _y + GetRelativeNodeY(iSpan[i]);

                    g.DrawLine(pen, new Point(x, y - 1), new Point(x, 2));
                }
            }
        }
    }

    private void DrawData(Graphics g, List<int> data, Pen pen)
    {
        if (data.Count > 0)
        {
            List<Point> points = [];
            ReadOnlySpan<int> spanData = CollectionsMarshal.AsSpan<int>(data);
            for (int i = spanData.Length - 1; i >= 0; i--)
            {
                int x = _x + i * (_width / spanData.Length);
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
        _throttlePen?.Dispose();
        _brakePen?.Dispose();
        _steeringPen?.Dispose();
        _tractionControlPen?.Dispose();
        _absPen?.Dispose();
    }
}
