using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.ACC.Overlays.Driving.InputTrace;

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
    private readonly Pen _tractionControlPen;
    private readonly Pen _absPen;

    public InputGraph(int x, int y, int width, int height, InputDataJob dataJob, InputTraceConfiguration config)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _dataJob = dataJob;
        _config = config;

        _throttlePen = new Pen(Color.FromArgb(_config.Colors.ThrottleOpacity, _config.Colors.ThrottleColor), _config.Chart.LineThickness);
        _brakePen = new Pen(Color.FromArgb(_config.Colors.BrakeOpacity, _config.Colors.BrakeColor), _config.Chart.LineThickness);
        _steeringPen = new Pen(Color.FromArgb(_config.Colors.SteeringOpacity, _config.Colors.SteeringColor), _config.Chart.LineThickness);
        _tractionControlPen = new Pen(Color.FromArgb(_config.Colors.TractionControlOpacity, _config.Colors.TractionControlColor), 1);
        _absPen = new Pen(Color.FromArgb(_config.Colors.AbsOpacity, _config.Colors.AbsColor), 2);

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

    public void Draw(Graphics g)
    {
        _cachedBackground?.Draw(g);

        g.SmoothingMode = SmoothingMode.HighQuality;

        if (_dataJob != null)
        {
            List<int> iData;
            List<bool> bData;

            if (_config.Chart.SteeringInput)
            {
                lock (_dataJob.Steering) iData = new(_dataJob.Steering);
                DrawData(g, iData, _steeringPen);
            }

            lock (_dataJob.Throttle) iData = new(_dataJob.Throttle);
            DrawData(g, iData, _throttlePen);
            if (_config.Chart.TractionControl)
            {
                lock (_dataJob.TC) bData = _dataJob.TC;
                DrawData(g, bData, iData, _tractionControlPen);
            }


            lock (_dataJob.Brake) iData = new(_dataJob.Brake);
            DrawData(g, iData, _brakePen);
            if (_config.Chart.Abs)
            {
                lock (_dataJob.ABS) bData = _dataJob.ABS;
                DrawData(g, bData, iData, _absPen);
            }
        }
    }

    private void DrawData(Graphics g, List<bool> bData, List<int> iData, Pen pen)
    {
        if (bData.Count > 0 && iData.Count > 0 && bData.Count == iData.Count)
        {
            var bSpan = CollectionsMarshal.AsSpan<bool>(bData);
            var iSpan = CollectionsMarshal.AsSpan<int>(iData);
            for (int i = 0; i < bSpan.Length - 1; i++)
            {
                if (bSpan[i])
                {
                    int x = _x + _width - i * (_width / bSpan.Length);
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
        _tractionControlPen?.Dispose();
        _absPen?.Dispose();
    }
}
