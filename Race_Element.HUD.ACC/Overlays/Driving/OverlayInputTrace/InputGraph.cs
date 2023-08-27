using RaceElement.Util.SystemExtensions;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using static RaceElement.HUD.ACC.Overlays.OverlayInputTrace.InputTraceOverlay;

namespace RaceElement.HUD.ACC.Overlays.OverlayInputTrace
{
    internal class InputGraph : IDisposable
    {
        private readonly int _x, _y;
        private readonly int _width, _height;
        private readonly InputTraceConfig _config;
        private readonly InputDataCollector _collector;

        private readonly CachedBitmap _cachedBackground;
        private readonly Pen _throttlePen;
        private readonly Pen _brakePen;
        private readonly Pen _steeringPen;

        public InputGraph(int x, int y, int width, int height, InputDataCollector collector, InputTraceConfig config)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _collector = collector;
            _config = config;

            _throttlePen = new Pen(Color.ForestGreen, _config.InfoPanel.LineThickness);
            _brakePen = new Pen(Color.Red, _config.InfoPanel.LineThickness);
            _steeringPen = new Pen(Color.FromArgb(190, Color.White), _config.InfoPanel.LineThickness);

            _cachedBackground = new CachedBitmap(_width + 1, _height + 1, g =>
            {
                if (config.InfoPanel.GridLines)
                {
                    using Pen linePen = new Pen(new SolidBrush(Color.FromArgb(90, Color.White)), 1);
                    for (int i = 1; i <= 9; i++)
                        g.DrawLine(linePen, new Point(0, i * _height / 10), new Point(_width, i * _height / 10));
                }

                Rectangle graphRect = new Rectangle(_x, _y, _width, _height);
                LinearGradientBrush gradientBrush = new LinearGradientBrush(graphRect, Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);
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

            if (this._config.InfoPanel.SteeringInput)
                DrawData(g, _collector.Steering, _steeringPen);

            DrawData(g, _collector.Throttle, _throttlePen);
            DrawData(g, _collector.Brake, _brakePen);
        }

        private void DrawData(Graphics g, LinkedList<int> Data, Pen pen)
        {
            if (Data.Count > 0)
            {
                List<Point> points = new List<Point>();
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
                    GraphicsPath path = new GraphicsPath();
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
            _throttlePen?.Dispose();
            _brakePen?.Dispose();
            _steeringPen?.Dispose();
        }
    }
}
