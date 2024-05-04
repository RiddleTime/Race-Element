using System.Drawing;
using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

internal sealed class TrackMapConfiguration : OverlayConfiguration
{
    [ConfigGrouping("Map", "Map options")]
    public MapGrouping Map { get; init; } = new();
    public sealed class MapGrouping
    {
        [ToolTip("Minimap max width")]
        [FloatRange(200.0f, 300.0f, 1.0f, 1)]
        public float MaxWidth { get; init; } = 300f;

        [ToolTip("Map lines thickness")]
        [FloatRange(1.0f, 10.0f, 0.1f, 1)]
        public float Thickness { get; init; } = 4.0f;

        [ToolTip("Rotation in degrees")]
        [FloatRange(-360.0f, 360.0f, 1.0f, 1)]
        public float Rotation { get; init; } = -90.0f;

        [ToolTip("Save map preview (Race Element directory -> Tracks)")]
        public bool SavePreview { get; init; } = false;

        [ToolTip("Map color")]
        public Color Color { get; init; } = Color.WhiteSmoke;
    }

    [ConfigGrouping("Car", "Car options")]
    public CarGrouping Car { get; init; } = new();
    public sealed class CarGrouping
    {
        [ToolTip("Show Cars number")]
        public bool ShowCarNumber { get; init; } = false;

        [ToolTip("Player car color")]
        public Color PlayerColor { get; init; } = Color.Red;

        [ToolTip("Others car color")]
        public Color OthersColor { get; init; } = Color.DarkGray;
    }

    [ConfigGrouping("Other", "Other options")]
    public OtherGrouping Other { get; init; } = new();
    public sealed class OtherGrouping
    {
        [ToolTip("Map dot size")]
        [FloatRange(5.0f, 32.0f, 1.0f, 1)]
        public float Dotsize { get; init; } = 15;

        [ToolTip("Font size")]
        [FloatRange(5.0f, 32.0f, 1.0f, 1)]
        public float FontSize { get; init; } = 10;
    }
}
