using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.CachedBitmapBenchmark;

[Overlay(Name = "CB Benchmark",
Description = "Shows info about the car in front and behind.",
OverlayType = OverlayType.Pitwall,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal class CachedBitmapBenchmarkOverlay : AbstractOverlay
{
    private const int InitialWidth = 300, InitialHeight = 250;

    private List<double> _notCached = [];
    private List<double> _cached = [];

    CachedBitmap _bitmap;
    public CachedBitmapBenchmarkOverlay(Rectangle rectangle) : base(rectangle, "CB Benchmark")
    {
        this.Width = InitialWidth;
        this.Height = InitialHeight;
        this.RefreshRateHz = 50;
    }

    public override void BeforeStart()
    {
        _bitmap = new CachedBitmap(100, 100, g => { RenderSomething(g, 100, 100); });
    }

    public override void BeforeStop()
    {
        _bitmap?.Dispose();
    }

    public override void Render(Graphics g)
    {
        g.Clear(Color.Transparent);

        var sw = Stopwatch.StartNew();
        RenderSomething(g, 100, 100);
        TimeSpan elapsed = sw.Elapsed;
        AddToBenchList(elapsed, ref _notCached);

        g.Clear(Color.Transparent);

        sw = Stopwatch.StartNew();
        _bitmap.Draw(g);
        TimeSpan elapsed2 = sw.Elapsed;
        AddToBenchList(elapsed2, ref _cached);

        if (_cached.Count % 100 == 0)
        {
            Trace.WriteLine($"Avg - raw: {_notCached.Average():F0} Ns, cached: {_cached.Average():F0} Ns");
            Trace.WriteLine($"Min - raw {_notCached.Min():F0} Ns, cached {_cached.Min():F0}");
            Trace.WriteLine($"Max - raw {_notCached.Max():F0} Ns, cached {_cached.Max():F0}");
        }
    }
    private void RenderSomething(Graphics g, int width, int height)
    {
        for (int i = 1; i < 10; i++)
            g.DrawRoundedRectangle(Pens.White, new Rectangle(0, 0, (width - 1) / i, (height - 1) / i), 2);
    }

    private void AddToBenchList(TimeSpan t, ref List<double> list)
    {
        list.Add(t.TotalNanoseconds);
    }
}
