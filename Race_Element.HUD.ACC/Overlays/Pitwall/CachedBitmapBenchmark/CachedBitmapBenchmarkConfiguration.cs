using RaceElement.HUD.Overlay.Configuration;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.CachedBitmapBenchmark;
internal sealed class CachedBitmapBenchmarkConfiguration : OverlayConfiguration
{
    [ConfigGrouping("Bench Settings", "Configure complexity of run")]
    public BenchGrouping Bench { get; init; } = new();
    public sealed class BenchGrouping
    {
        [ToolTip("The complexity of the drawing set by the amount of iterations.")]
        [IntRange(1, 100, 1)]
        public int ComplexityIterations { get; init; } = 4;

        [ToolTip("The benchmark iterations per second.")]
        [IntRange(1, 50, 1)]
        public int IterationsPerSecond { get; init; } = 20;

        [IntRange(8, 256, 8)]
        public int DrawingDimension { get; init; } = 64;

        public CompositingQuality CompositingQuality { get; init; } = CompositingQuality.Default;
        public SmoothingMode SmoothingMode { get; init; } = SmoothingMode.Default;
    }
}
