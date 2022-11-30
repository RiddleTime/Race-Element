using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace ACCManager.HUD.ACC.Overlays.OverlaySlipAngle
{
    internal class OversteerGraph : IDisposable
    {
        private int _x, _y;
        private int _width, _height;
        private readonly OversteerDataCollector _collector;

        private readonly CachedBitmap _cachedBackground;

        public OversteerGraph(int x, int y, int width, int height, OversteerDataCollector collector)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _collector = collector;

            _cachedBackground = new CachedBitmap(_width, _height, g =>
            {
                Rectangle graphRect = new Rectangle(0, 0, _width, _height);
                LinearGradientBrush gradientBrush = new LinearGradientBrush(graphRect, Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);
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
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.GammaCorrected;

            if (_cachedBackground != null)
                _cachedBackground.Draw(g, _x, _y, _width, _height);

            DrawData(g, _collector.OversteerData, Color.OrangeRed);
            DrawData(g, _collector.UndersteerData, Color.DeepSkyBlue);


            g.SmoothingMode = previous;
        }

        private void DrawData(Graphics g, LinkedList<float> Data, Color color)
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
                    g.DrawPath(new Pen(color, 1f), path);
                }
            }
        }

        public void Dispose()
        {
            _cachedBackground.Dispose();
        }
    }
}
