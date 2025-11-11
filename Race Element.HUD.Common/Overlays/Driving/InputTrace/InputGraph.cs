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
    private readonly Pen _steeringPen;
    private readonly Pen _tractionControlPen;
    private readonly Pen _absPen;

    // Chunked caching - each input trace is cached separately
    private CachedBitmap? _cachedThrottleGraph;
    private CachedBitmap? _cachedBrakeGraph;
    private CachedBitmap? _cachedSteeringGraph;
    private CachedBitmap? _cachedTractionControlGraph;
    private CachedBitmap? _cachedAbsGraph;

    // Reusable buffers to avoid per-frame allocations
    private readonly List<int> _throttleBuffer = [];
    private readonly List<int> _brakeBuffer = [];
    private readonly List<int> _steeringBuffer = [];
    private readonly List<bool> _tractionControlBuffer = [];
    private readonly List<bool> _absBuffer = [];
    private readonly List<Point> _pointsBuffer = [];

    // Cache invalidation tracking
    private int _lastDataHash = -1;
    private int _framesSinceLastCacheUpdate = 0;
    private const int MAX_FRAMES_BETWEEN_CACHE_UPDATES = 6; // Update cache every 6 frames at 60fps = 10 updates/sec

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

    /// <summary>
    /// Fast hash of buffer contents to detect meaningful changes
    /// </summary>
    private int ComputeDataHash(List<int> throttle, List<int> brake, List<int> steering, List<bool> tc, List<bool> abs)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + throttle.Count;
            hash = hash * 31 + brake.Count;
            hash = hash * 31 + steering.Count;
            hash = hash * 31 + tc.Count;
            hash = hash * 31 + abs.Count;

            // Sample first, middle, and last elements to detect changes without hashing entire buffers
            if (throttle.Count > 0)
            {
                hash = hash * 31 + throttle[0];
                if (throttle.Count > 1)
                    hash = hash * 31 + throttle[throttle.Count - 1];
            }
            if (brake.Count > 0)
            {
                hash = hash * 31 + brake[0];
                if (brake.Count > 1)
                    hash = hash * 31 + brake[brake.Count - 1];
            }

            return hash;
        }
    }

    public void Draw(Graphics g, ConcurrentQueue<InputsData> data)
    {
        _cachedBackground?.Draw(g);

        g.SmoothingMode = SmoothingMode.HighQuality;

        // Populate buffers from queue
        _throttleBuffer.Clear();
        _brakeBuffer.Clear();
        _steeringBuffer.Clear();
        _tractionControlBuffer.Clear();
        _absBuffer.Clear();

        foreach (var item in data)
        {
            _throttleBuffer.Add(item.Throttle);
            _brakeBuffer.Add(item.Brake);
            _steeringBuffer.Add(item.Steering);
            _tractionControlBuffer.Add(item.TractionControlActivation);
            _absBuffer.Add(item.AbsActivation);
        }

        // Decide whether to update caches
        int currentHash = ComputeDataHash(_throttleBuffer, _brakeBuffer, _steeringBuffer, _tractionControlBuffer, _absBuffer);
        _framesSinceLastCacheUpdate++;
        
        bool shouldUpdateCache = currentHash != _lastDataHash || _framesSinceLastCacheUpdate >= MAX_FRAMES_BETWEEN_CACHE_UPDATES;

        if (shouldUpdateCache)
        {
            _lastDataHash = currentHash;
            _framesSinceLastCacheUpdate = 0;

            // Update throttle cache
            _cachedThrottleGraph?.Dispose();
            _cachedThrottleGraph = new(_width + 1, _height + 1, g =>
            {
                DrawData(g, _throttleBuffer, _throttlePen);
            });

            // Update steering cache
            if (_config.Chart.SteeringInput)
            {
                _cachedSteeringGraph?.Dispose();
                _cachedSteeringGraph = new(_width + 1, _height + 1, g =>
                {
                    DrawData(g, _steeringBuffer, _steeringPen);
                });
            }

            // Update traction control cache
            if (_config.TractionControl.TractionControl)
            {
                _cachedTractionControlGraph?.Dispose();
                _cachedTractionControlGraph = new(_width + 1, _height + 1, g =>
                {
                    DrawData(g, _tractionControlBuffer, _throttleBuffer, _tractionControlPen);
                });
            }

            // Update brake cache
            _cachedBrakeGraph?.Dispose();
            _cachedBrakeGraph = new(_width + 1, _height + 1, g =>
            {
                DrawData(g, _brakeBuffer, _brakePen);
            });

            // Update ABS cache
            if (_config.Abs.Abs)
            {
                _cachedAbsGraph?.Dispose();
                _cachedAbsGraph = new(_width + 1, _height + 1, g =>
                {
                    DrawData(g, _absBuffer, _brakeBuffer, _absPen);
                });
            }
        }

        // Draw all cached graphs (very fast operation)
        _cachedSteeringGraph?.Draw(g);
        _cachedThrottleGraph?.Draw(g);
        _cachedTractionControlGraph?.Draw(g);
        _cachedBrakeGraph?.Draw(g);
        _cachedAbsGraph?.Draw(g);
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
        _cachedThrottleGraph?.Dispose();
        _cachedBrakeGraph?.Dispose();
        _cachedSteeringGraph?.Dispose();
        _cachedTractionControlGraph?.Dispose();
        _cachedAbsGraph?.Dispose();
        _throttlePen?.Dispose();
        _brakePen?.Dispose();
        _steeringPen?.Dispose();
        _tractionControlPen?.Dispose();
        _absPen?.Dispose();
    }
}
