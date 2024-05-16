using System.Drawing;
using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public sealed class TrackMapConfiguration : OverlayConfiguration
{
    [ConfigGrouping("General", "General options")]
    public GeneralGrouping General { get; init; } = new();
    public sealed class GeneralGrouping
    {
        [ToolTip("Map scale factor")]
        [FloatRange(1.0f, 3.0f, 0.01f, 2)]
        public float ScaleFactor { get; init; } = 1.0f;

        [ToolTip("Map lines thickness")]
        [FloatRange(1.0f, 10.0f, 0.1f, 1)]
        public float Thickness { get; init; } = 3.0f;

        [ToolTip("Map rotation in degrees")]
        [FloatRange(-360.0f, 360.0f, 1.0f, 1)]
        public float Rotation { get; init; } = -90.0f;

        [ToolTip("Lapped distance threshold in meters")]
        [FloatRange(0, 500, 1.0f, 1)]
        public float LappedThreshold { get; init; } = 150;

        [ToolTip("Cars going at less than X Km/h will appear in yellow")]
        [IntRange(0, 50, 1)]
        public int KmhThreshold { get; init; } = 45;

        [ToolTip("Show Cars number")]
        public bool ShowCarNumber { get; init; } = true;
    }

    [ConfigGrouping("Pitstop", "Pit stop options")]
    public PitstopGrouping Pitstop { get; init; } = new();
    public sealed class PitstopGrouping
    {
        [ToolTip("Show pit stop on map")]
        public bool ShowPitStop { get; init; } = true;

        [ToolTip("Fixed pit time in seconds (default game is 30 seconds)")]
        [IntRange(0, 120, 1)]
        public int FixedPitTime { get; init; } = 30;

        [ToolTip("Additional pit time in seconds (pit lane entry/exit, errors, etc)")]
        [IntRange(0, 120, 1)]
        public int PitAdditionalTime { get; init; } = 5;
    }

    [ConfigGrouping("Other", "Other options")]
    public OthersGrouping Others { get; init; } = new();
    public sealed class OthersGrouping
    {
        [ToolTip("Refresh interval (times per second)")]
        [IntRange(8, 24, 1)]
        public int RefreshInterval { get; init; } = 18;

        [ToolTip("Font size for car number")]
        [FloatRange(5.0f, 32.0f, 1.0f, 1)]
        public float FontSize { get; init; } = 10;

        [ToolTip("Car size on the map")]
        [FloatRange(12.0f, 32.0f, 1.0f, 1)]
        public float CarSize { get; init; } = 12;

        [ToolTip("Save map preview (Race Element directory -> Tracks)")]
        public bool SavePreview { get; init; } = false;
    }

    [ConfigGrouping("Colors", "Colors options")]
    public ColorGrouping Colors { get; init; } = new();
    public sealed class ColorGrouping
    {
        [ToolTip("Map color")]
        public Color Map { get; init; } = Color.FromArgb(255, 245, 245, 245);

        [ToolTip("Player car color")]
        public Color Player { get; init; } = Color.FromArgb(255, 255, 0, 0);

        [ToolTip("Default car color (no player / no lapped)")]
        public Color Default { get; init; } = Color.FromArgb(255, 211, 211, 211);

        [ToolTip("Leader car color (no player)")]
        public Color Leader { get; init; } = Color.FromArgb(255, 46, 139, 87);

        [ToolTip("Cars lapped by player color")]
        public Color PlayerLappedOthers { get; init; } = Color.FromArgb(255, 70, 130, 180);

        [ToolTip("Other lapped by player color")]
        public Color OthersLappedPlayer { get; init; } = Color.FromArgb(255, 255, 140, 0);

        [ToolTip("Improving lap car color (no player)")]
        public Color ImprovingLap { get; init; } = Color.FromArgb(255, 46, 139, 87);

        [ToolTip("Pit stop color")]
        public Color PitStop { get; init; } = Color.FromArgb(255, 186, 85, 211);

        [ToolTip("Pit stop with damage color")]
        public Color PitStopWithDamage { get; init; } = Color.FromArgb(255, 147, 112, 219);
    }

    public TrackMapConfiguration() => this.GenericConfiguration.AllowRescale = false;
}
