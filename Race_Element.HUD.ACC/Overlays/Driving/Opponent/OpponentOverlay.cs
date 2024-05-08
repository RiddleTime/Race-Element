using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Documents;

namespace RaceElement.HUD.ACC.Overlays.OverlayOpponent;

#if DEBUG
[Overlay(Name = "Opponent", Description = "Shows info about the car in front and behind.", OverlayType = OverlayType.Drive, Version = 1.00)]
#endif
internal class OpponentOverlay : AbstractOverlay
{
    private const int InitialWidth = 300, InitialHeight = 250;

    private List<double> _notCached = [];
    private List<double> _cached = [];

    CachedBitmap _bitmap;
    public OpponentOverlay(Rectangle rectangle) : base(rectangle, "Opponent")
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

        double avgUncached = _notCached.Average();
        double avgCached = _cached.Average();
        Trace.WriteLine($"cycles: {_cached.Count},  raw: {avgUncached:F0} Ns, cached: {avgCached:F0} Ns");
    }
    private void RenderSomething(Graphics g, int width, int height)
    {
        for (int i = 1; i < 10; i++)
            g.DrawRoundedRectangle(Pens.White, new Rectangle(0, 0, width / i, height / i), 2);
    }

    private void AddToBenchList(TimeSpan t, ref List<double> list)
    {
        list.Add(t.TotalNanoseconds);
    }
}
