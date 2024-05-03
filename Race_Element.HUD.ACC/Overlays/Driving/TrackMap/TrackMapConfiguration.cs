using System.Drawing;
using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

internal sealed class TrackMapConfiguration : OverlayConfiguration
{
    [ConfigGrouping("Map", "Map options")]
    public MapGrouping Map { get; init; } = new();
    public sealed class MapGrouping
    {
        [ToolTip("Map scale (between 0.1 and 1.0")]
        [FloatRange(0.1f, 1.0f, 0.1f, 1)]
        public float Scale { get; init; } = 0.4f;

        [ToolTip("Map thickness (between 1.0 and 5.0")]
        [FloatRange(1.0f, 5.0f, 0.1f, 1)]
        public float Thickness { get; init; } = 1.0f;

        [ToolTip("Save map preview (Race Element directory -> Tracks)")]
        public bool SavePreview { get; init; } = false;

        [ToolTip("Map color")]
        public Color MapColor { get; init; } = Color.DarkGray;
    }
}
