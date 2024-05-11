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
        public bool ShowCarNumber { get; init; } = true;

        [ToolTip("Show pit stop on map")]
        public bool ShowPitStop { get; init; } = true;

        [ToolTip("Player car color")]
        public Color PlayerColor { get; init; } = Color.Red;

        [ToolTip("Default car color (no player / no lapped)")]
        public Color DefaultColor { get; init; } = Color.LightGray;

        [ToolTip("Leader car color (no player)")]
        public Color LeaderColor { get; init; } = Color.SeaGreen;

        [ToolTip("Improving lap car color (no player)")]
        public Color ImprovingLapColor { get; init; } = Color.SeaGreen;


        [ToolTip("Pit stop color")]
        public Color PitStopColor { get; init; } = Color.MediumOrchid;

        [ToolTip("Pit stop with damage color")]
        public Color PitStopWithDamageColor { get; init; } = Color.MediumPurple;
    }

    [ConfigGrouping("Other", "Other options")]
    public OtherGrouping Other { get; init; } = new();
    public sealed class OtherGrouping
    {
        [ToolTip("Car size on the map")]
        [FloatRange(5.0f, 32.0f, 1.0f, 1)]
        public float CarSize { get; init; } = 15;

        [ToolTip("Font size for car number")]
        [FloatRange(5.0f, 32.0f, 1.0f, 1)]
        public float FontSize { get; init; } = 10;

        [ToolTip("Lapped distance threshold in meters")]
        [FloatRange(0, 500, 1.0f, 1)]
        public float LappedThreshold { get; init; } = 150;

        [ToolTip("Cars lapped by player color")]
        public Color PlayerLappedOthersColor { get; init; } = Color.SteelBlue;

        [ToolTip("Other lapped by player color")]
        public Color OthersLappedPlayerColor { get; init; } = Color.DarkOrange;
    }

    public TrackMapConfiguration() => this.GenericConfiguration.AllowRescale = false;
}
