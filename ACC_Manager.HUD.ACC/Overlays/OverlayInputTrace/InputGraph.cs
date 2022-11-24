using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using static ACCManager.HUD.ACC.Overlays.OverlayInputTrace.InputTraceOverlay;

namespace ACCManager.HUD.ACC.Overlays.OverlayInputTrace
{
    internal class InputGraph : IDisposable
    {
        private int _x, _y;
        private int _width, _height;
        private readonly InputTraceConfig _config;
        private readonly InputDataCollector _collector;

        private readonly CachedBitmap _cachedBackground;

        public InputGraph(int x, int y, int width, int height, InputDataCollector collector, InputTraceConfig config)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _collector = collector;
            _config = config;

            _cachedBackground = new CachedBitmap(_width, _height, g =>
            {
                Rectangle graphRect = new Rectangle(_x, _y, _width, _height);
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(196, Color.Black)), graphRect, 3);
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
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (_cachedBackground != null)
                _cachedBackground.Draw(g);

            if (this._config.InfoPanel.SteeringInput)
                DrawData(g, _collector.Steering, Color.FromArgb(190, Color.White));

            DrawData(g, _collector.Throttle, Color.ForestGreen);
            DrawData(g, _collector.Brake, Color.Red);


            g.SmoothingMode = previous;
        }

        private void DrawData(Graphics g, LinkedList<int> Data, Color color)
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
                            y.ClipMax(_y + _height);

                            if (x < _x)
                                break;

                            points.Add(new Point(x, y));
                        }

                    }

                if (points.Count > 0)
                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddLines(points.ToArray());
                    g.DrawPath(new Pen(color, 2f), path);
                }
            }
        }

        public void Dispose()
        {
            _cachedBackground.Dispose();
        }
    }
}
