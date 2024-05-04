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

        [ToolTip("Map lines thickness (between 1.0 and 10.0")]
        [FloatRange(1.0f, 10.0f, 0.1f, 1)]
        public float Thickness { get; init; } = 4.0f;

        [ToolTip("Map rotation (between -360º and 360º")]
        [FloatRange(-360.0f, 360.0f, 1.0f, 1)]
        public float Rotation { get; init; } = 0.0f;

        [ToolTip("Save map preview (Race Element directory -> Tracks)")]
        public bool SavePreview { get; init; } = false;

        [ToolTip("Map color")]
        public Color Color { get; init; } = Color.WhiteSmoke;
    }

    [ConfigGrouping("Car", "Car options")]
    public CarGrouping Car { get; init; } = new();
    public sealed class CarGrouping
    {
        [ToolTip("Cars dot size (between 5.0 and 20.0")]
        [FloatRange(5.0f, 20.0f, 1.0f, 1)]
        public float Scale { get; init; } = 10.0f;

        [ToolTip("Player car color")]
        public Color PlayerColor { get; init; } = Color.DarkRed;

        [ToolTip("Others car color")]
        public Color OthersColor { get; init; } = Color.DarkGray;
    }
}
