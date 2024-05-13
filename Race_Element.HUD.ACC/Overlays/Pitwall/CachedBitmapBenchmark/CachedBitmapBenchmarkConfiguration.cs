using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.CachedBitmapBenchmark;
internal sealed class CachedBitmapBenchmarkConfiguration : OverlayConfiguration
{
    [ConfigGrouping("Bench Settings", "Configure complexity of run")]
    public BenchGrouping Bench { get; init; } = new();
    public sealed class BenchGrouping
    {
        [ToolTip("The complexity of the drawing set by the amount of iterations.")]
        [IntRange(1, 100, 1)]
        public int ComplexityIterations { get; init; } = 2;
    }
}
