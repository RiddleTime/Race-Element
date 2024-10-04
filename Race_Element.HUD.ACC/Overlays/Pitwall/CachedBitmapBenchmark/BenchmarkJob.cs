using RaceElement.Core.Jobs.Loop;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.CachedBitmapBenchmark
{
    internal sealed class BenchmarkJob(int actions, SmoothingMode smoothingMode, CompositingQuality compositingQuality, int drawingDimension) : AbstractLoopJob
    {
        public ConcurrentBag<double> _notCached = [];
        public ConcurrentBag<double> _cached = [];

        private CachedBitmap _cachedBitmap;
        private CachedBitmap _benchmarkRenderCached;
        private SolidBrush _brush;

        public override void BeforeRun()
        {
            int brushAlpha = 255 / actions;
            brushAlpha.Clip(10, 240);
            _brush = new(Color.FromArgb(brushAlpha, Color.LimeGreen));

            _benchmarkRenderCached = new(drawingDimension, drawingDimension, g => RenderSomething(g, drawingDimension, drawingDimension, actions));

            _cachedBitmap = new CachedBitmap(drawingDimension, drawingDimension, g =>
            {
                g.InterpolationMode = InterpolationMode.Default;

                g.Clear(Color.Transparent);
                var sw = Stopwatch.StartNew();
                RenderSomething(g, drawingDimension, drawingDimension, actions);
                TimeSpan elapsed = sw.Elapsed;
                sw.Stop();
                AddToBenchBag(elapsed, ref _notCached);

                g.CompositingQuality = CompositingQuality.Default;
                g.SmoothingMode = SmoothingMode.Default;

                g.Clear(Color.Transparent);
                sw = Stopwatch.StartNew();
                _benchmarkRenderCached.Draw(g);
                TimeSpan elapsed2 = sw.Elapsed;
                sw.Stop();
                AddToBenchBag(elapsed2, ref _cached);
            });
        }

        public override void AfterCancel()
        {
            _cachedBitmap?.Dispose();
            _benchmarkRenderCached?.Dispose();
            _brush?.Dispose();
        }
        public override void RunAction()
        {
            _cachedBitmap.Render();
        }

        private void RenderSomething(Graphics g, int width, int height, int actions)
        {
            g.SmoothingMode = smoothingMode;
            g.CompositingQuality = compositingQuality;
            for (int i = 0; i < actions; i++)
            {
                int divider = i;
                divider.ClipMin(1);
                g.FillRoundedRectangle(_brush, new Rectangle(0, 0, (width - 1) / divider, (height - 1) / divider), 2);
            }
        }

        private static void AddToBenchBag(TimeSpan t, ref ConcurrentBag<double> bag) => bag.Add(t.TotalMilliseconds);
    }
}
