using RaceElement.Core.Jobs.LoopJob;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.CachedBitmapBenchmark
{
    internal sealed class BenchmarkJob(int actions, SmoothingMode smoothingMode, CompositingQuality compositingQuality) : AbstractLoopJob
    {
        private readonly object _lock = new();
        public List<double> _notCached = [];
        public List<double> _cached = [];

        private CachedBitmap _cachedBitmap;
        private CachedBitmap _benchmarkRenderCached;
        private SolidBrush _brush;
        private const int Width = 300;
        private const int Height = 300;

        public override void BeforeRun()
        {
            int brushAlpha = 255 / actions;
            brushAlpha.Clip(10, 240);
            _brush = new(Color.FromArgb(brushAlpha, Color.LimeGreen));

            _benchmarkRenderCached = new(Width, Height, g => RenderSomething(g, Width, Height, actions));

            _cachedBitmap = new CachedBitmap(Width, Height, g =>
            {
                g.InterpolationMode = InterpolationMode.Default;

                g.Clear(Color.Transparent);
                var sw = Stopwatch.StartNew();
                RenderSomething(g, Width, Height, actions);
                TimeSpan elapsed = sw.Elapsed;
                sw.Stop();
                lock (_lock) AddToBenchList(elapsed, ref _notCached);

                g.CompositingQuality = CompositingQuality.Default;
                g.SmoothingMode = SmoothingMode.Default;

                g.Clear(Color.Transparent);
                sw = Stopwatch.StartNew();
                _benchmarkRenderCached.Draw(g);
                TimeSpan elapsed2 = sw.Elapsed;
                sw.Stop();
                lock (_lock) AddToBenchList(elapsed2, ref _cached);
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

        private static void AddToBenchList(TimeSpan t, ref List<double> list) => list.Add(t.TotalMilliseconds);
    }
}
