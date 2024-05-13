using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.CachedBitmapBenchmark;

[Overlay(Name = "CB Benchmark",
Description = "Benches Drawing.",
OverlayType = OverlayType.Pitwall,
Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal sealed class CachedBitmapBenchmarkOverlay : AbstractOverlay
{
    private readonly CachedBitmapBenchmarkConfiguration _config = new();

    private BenchmarkJob _benchmarkJob;

    private InfoPanel _panel;

    public CachedBitmapBenchmarkOverlay(Rectangle rectangle) : base(rectangle, "CB Benchmark")
    {
        this.RefreshRateHz = 1;
    }

    public sealed override void BeforeStart()
    {
        Width = 700;
        Height = 65;
        _panel = new(10, 700);
        if (IsPreviewing) return;

        _benchmarkJob = new(_config.Bench.ComplexityIterations, _config.Bench.SmoothingMode, _config.Bench.CompositingQuality)
        {
            IntervalMillis = 1000 / _config.Bench.IterationsPerSecond
        };
        _benchmarkJob.Run();
    }

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        _benchmarkJob?.CancelJoin();
    }

    public sealed override bool ShouldRender() => true;

    public sealed override void Render(Graphics g)
    {
        if (IsPreviewing) return;

        if (_benchmarkJob._cached.Count > 2)
        {
            _panel.AddLine("", $"S: {_config.Bench.SmoothingMode}, Q: {_config.Bench.CompositingQuality}, P/S: {_config.Bench.IterationsPerSecond}");
            _panel.AddLine("", $"Iterations: {_benchmarkJob._notCached.Count} - Complexity {_config.Bench.ComplexityIterations}");
            _panel.AddLine("Raw MS", GetStats(_benchmarkJob._notCached));
            _panel.AddLine("CB MS", GetStats(_benchmarkJob._cached));
            _panel.Draw(g);
        }
    }

    private static string GetStats(List<double> data)
    {
        var metrics = CalculateMetrics(data);
        StringBuilder sb = new();
        sb.Append($"Min: {metrics.min:F4}");
        sb.Append($", Avg: {metrics.mean:F4}");
        sb.Append($", Max: {metrics.max:F4}");
        sb.Append($", Median: {metrics.median:F4}");
        sb.Append($", StDev: {metrics.std:F4}");
        return sb.ToString();
    }

    private static (double min, double max, double mean, double median, double std) CalculateMetrics(List<double> list)
    {
        var mean = list.Average();
        var std = Math.Sqrt(list.Aggregate(0.0, (a, x) => a + (x - mean) * (x - mean)) / list.Count);
        var sorted = list.OrderBy(x => x).ToList();
        var median = sorted.Count % 2 == 0 ? (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2 : sorted[sorted.Count / 2];
        return (sorted[0], sorted[^1], mean, median, std);
    }

}
