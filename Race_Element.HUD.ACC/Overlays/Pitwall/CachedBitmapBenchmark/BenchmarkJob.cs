using RaceElement.Core.Jobs.LoopJob;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.CachedBitmapBenchmark
{
    internal sealed class BenchmarkJob(int actions, SmoothingMode smoothingMode, CompositingQuality compositingQuality) : AbstractLoopJob
    {
        private readonly object _lock = new();
        public List<double> _notCached = [];
        public List<double> _cached = [];

        private CachedBitmap _cachedBitmap;
        private CachedBitmap _benchmarkRenderCached;
        private Pen _pen;
        private const int Width = 100;
        private const int Height = 100;

        public override void BeforeRun()
        {
            _pen = new(Color.FromArgb(130, Color.LimeGreen));
            _benchmarkRenderCached = new(Width, Height, g => RenderSomething(g, Width, Height, actions));

            _cachedBitmap = new CachedBitmap(Width, Height, g =>
            {
                g.Clear(Color.Transparent);
                var sw = Stopwatch.StartNew();
                RenderSomething(g, Width, Height, actions);
                TimeSpan elapsed = sw.Elapsed;
                sw.Stop();
                lock (_lock) AddToBenchList(elapsed, ref _notCached);

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
            _pen?.Dispose();
        }
        public override void RunAction()
        {
            try
            {
                _cachedBitmap.Render();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }

        private void RenderSomething(Graphics g, int width, int height, int actions)
        {
            g.SmoothingMode = smoothingMode;
            g.CompositingQuality = compositingQuality;
            for (int i = 1; i < actions; i++)
                g.DrawRoundedRectangle(_pen, new Rectangle(0, 0, (width - 1) / i, (height - 1) / i), 2);
        }

        private static void AddToBenchList(TimeSpan t, ref List<double> list) => list.Add(t.TotalMilliseconds);
    }
}
