using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.OverlayLapDeltaBar;

internal sealed class LapTimeDeltaConfiguration : OverlayConfiguration
{
    [ConfigGrouping("Delta", "Adjust how the delta is displayed")]
    public DeltaGrouping Delta { get; init; } = new DeltaGrouping();
    public sealed class DeltaGrouping
    {
        [ToolTip("Sets the maximum range in seconds for the delta bar.")]
        [FloatRange(0.02f, 9.98f, 0.02f, 2)]
        public float MaxDelta { get; init; } = 2;

        [ToolTip("Sets the amount of decimals.")]
        [IntRange(1, 3, 1)]
        public int Decimals { get; init; } = 3;

        [ToolTip("Sets the size of the font.")]
        [IntRange(12, 30, 2)]
        public int FontSize { get; init; } = 20;

        [ToolTip("Hide the Lap Delta HUD during a Race session.")]
        public bool HideForRace { get; init; } = false;

        [ToolTip("Show the Lap Delta HUD when spectating.")]
        public bool Spectator { get; init; } = true;
    }

    [ConfigGrouping("Bar", "Adjust bar behavior.")]
    public BarGrouping Bar { get; init; } = new BarGrouping();
    public sealed class BarGrouping
    {
        [ToolTip("Sets the Width of the Delta Bar.")]
        [IntRange(180, 800, 10)]
        public int Width { get; init; } = 300;

        [ToolTip("Sets the Height of the Delta Bar.")]
        [IntRange(20, 60, 2)]
        public int Height { get; init; } = 32;

        [IntRange(1, 9, 1)]
        public int Roundness { get; init; } = 5;
    }

    [ConfigGrouping("Colors", "Adjust Colors.")]
    public ColorsGrouping Colors { get; init; } = new ColorsGrouping();
    public sealed class ColorsGrouping
    {
        [ToolTip("Sets the color when the delta is negative (faster).")]
        public Color FasterColor { get; init; } = Color.FromArgb(255, Color.LimeGreen);
        [IntRange(75, 255, 1)]
        public int FasterOpacity { get; init; } = 255;

        [ToolTip("Sets the color when the delta is positive (slower).")]
        public Color SlowerColor { get; init; } = Color.FromArgb(255, Color.OrangeRed);
        [IntRange(75, 255, 1)]
        public int SlowerOpacity { get; init; } = 255;
    }

    public LapTimeDeltaConfiguration() => this.GenericConfiguration.AllowRescale = true;
}
