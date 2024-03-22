using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

internal sealed class GForceTraceConfiguration : OverlayConfiguration
{
    public GForceTraceConfiguration() => GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Chunks", "Adjust chunk settings")]
    public ChunkGrouping Chunks { get; init; } = new();
    public sealed class ChunkGrouping
    {
        [IntRange(1, 200, 1)]
        public int MaxChunks { get; init; } = 10;

        [FloatRange(1, 3, 0.1f, 1)]
        public float MaxLatG { get; init; } = 3f;

        [FloatRange(1, 3, 0.1f, 1)]
        public float MaxLongG { get; init; } = 2.5f;
    }
}

