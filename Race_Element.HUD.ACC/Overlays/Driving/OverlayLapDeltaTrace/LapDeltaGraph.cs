using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using static RaceElement.HUD.ACC.Overlays.Driving.OverlayLapDeltaGraph.LapDeltaTraceOverlay;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayLapDeltaGraph
{
    internal class LapDeltaGraph : IDisposable
    {
        private readonly int _x, _y;
        private readonly int _width, _height;
        private readonly LapDeltaDataCollector _collector;

        private readonly CachedBitmap _cachedBackground;
        private readonly Pen _penPositiveDelta;
        private readonly Pen _penNegativeDelta;
        private readonly Pen _penZeroDelta;

        public LapDeltaGraph(int x, int y, int width, int height, LapDeltaDataCollector collector, LapDeltaTraceConfiguration config)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _collector = collector;

            _penPositiveDelta = new Pen(Color.OrangeRed, config.Chart.LineThickness);
            _penNegativeDelta = new Pen(Color.LimeGreen, config.Chart.LineThickness);
            _penZeroDelta = new Pen(Color.White, config.Chart.LineThickness);

            _cachedBackground = new CachedBitmap(_width + 1, _height + 1, g =>
            {
                Rectangle graphRect = new Rectangle(0, 0, _width, _height);
                LinearGradientBrush gradientBrush = new LinearGradientBrush(graphRect, Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);
                g.FillRoundedRectangle(gradientBrush, graphRect, 3);
                g.DrawRoundedRectangle(new Pen(Color.FromArgb(196, Color.Black)), graphRect, 3);
            });
        }

        private int GetRelativeNodeY(float value)
        {
            double range = _collector.MaxDelta - 0;
            double percentage = 1d - (value - 0) / range;
            return (int)(percentage * (_height - _height / 5))
                    + _height / 10;
        }

        public void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;

            _cachedBackground?.Draw(g, _x, _y, _width, _height);

            DrawData(g, _collector.PositiveDeltaData, _penPositiveDelta);
            DrawData(g, _collector.NegativeDeltaData, _penNegativeDelta);
            DrawNoData(g, _collector.PositiveDeltaData, _collector.NegativeDeltaData, _penZeroDelta);
        }

        private void DrawNoData(Graphics g, LinkedList<float> data1, LinkedList<float> data2, Pen pen)
        {
            if (data1.Count > 0 && data2.Count > 0 && data1.Count == data2.Count)
            {
                List<Point[]> points = new List<Point[]>();
                lock (data1)
                    lock (data2)
                    {
                        List<Point> subList = new List<Point>();

                        bool first = true;
                        for (int i = 0; i < data1.Count - 1; i++)
                        {
                            float one = data1.ElementAt(i);
                            float two = data2.ElementAt(i);

                            if (one != 0 || two != 0)
                            {
                                if (subList.Any())
                                {
                                    int x = _x + _width - i * (_width / data1.Count);
                                    int y = _y + GetRelativeNodeY(0);
                                    subList.Add(new Point(x, y));

                                    points.Add(subList.ToArray());
                                }

                                subList.Clear();
                            }
                            else
                            {
                                if (subList.Count == 0 && i > 0)
                                {
                                    int xBefore = _x + _width - i - 1 * (_width / data1.Count);
                                    int yBefore = _y + GetRelativeNodeY(0);
                                    if (xBefore < _x)
                                        break;

                                    subList.Add(new Point(xBefore, yBefore));
                                }

                                int x = _x + _width - i * (_width / data1.Count);
                                int y = _y + GetRelativeNodeY(0);

                                if (x < _x)
                                    break;

                                subList.Add(new Point(x, y));
                            }
                        }

                        if (subList.Any())
                            points.Add(subList.ToArray());
                    }

                if (points.Count > 0)
                    foreach (Point[] subList in points)
                    {
                        if (!subList.Any())
                            continue;

                        using GraphicsPath path = new GraphicsPath();
                        path.AddLines(subList);
                        g.DrawPath(pen, path);
                        path?.Dispose();
                    }
            }
        }


        private void DrawData(Graphics g, LinkedList<float> Data, Pen pen)
        {

            if (Data.Count > 0)
            {
                List<Point[]> points = new List<Point[]>();
                lock (Data)
                {
                    List<Point> subList = new List<Point>();

                    float previous = 0;
                    for (int i = 0; i < Data.Count - 1; i++)
                    {
                        float current = Data.ElementAt(i);

                        if (current == 0 && previous == 0)
                        {
                            if (subList.Count >= 2)
                                points.Add(subList.ToArray());

                            subList.Clear();
                        }
                        else if (current != 0)
                        {
                            int x = _x + _width - i * (_width / Data.Count);
                            int y = _y + GetRelativeNodeY(current);

                            if (x < _x)
                                break;

                            subList.Add(new Point(x, y));
                        }

                        previous = current;
                    }

                    if (subList.Any())
                        points.Add(subList.ToArray());
                }

                if (points.Count > 0)
                    foreach (Point[] subList in points)
                    {
                        if (!subList.Any())
                            continue;

                        using GraphicsPath path = new GraphicsPath();
                        path.AddLines(subList);
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
            _penPositiveDelta?.Dispose();
            _penNegativeDelta?.Dispose();
        }
    }
}
