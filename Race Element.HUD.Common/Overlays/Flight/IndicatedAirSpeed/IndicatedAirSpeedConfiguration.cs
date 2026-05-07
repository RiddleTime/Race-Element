using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Flight.IndicatedAirSpeed;

internal sealed class IndicatedAirSpeedConfiguration : OverlayConfiguration
{
    public enum RpmTextFont { Roboto, Conthrax, Obitron, Segoe }
    public enum UnitChoice { Knots, KilometersPerHour, MilesPerHour, MetersPerSecond, FeetPerSecond }

    public IndicatedAirSpeedConfiguration() => GenericConfiguration.AllowRescale = false;

    [ConfigGrouping("General", "General options")]
    public GeneralGrouping General { get; init; } = new();
    public sealed class GeneralGrouping
    {
        [ToolTip("Unit Type")]
        public UnitChoice Units { get; init; } = UnitChoice.Knots;

        [ToolTip("Size of the font")]
        [FloatRange(12.0f, 90.0f, 0.5f, 1)]
        public float FontSize { get; init; } = 25.5f;

        [ToolTip("Change the Font")]
        public RpmTextFont Font { get; init; } = RpmTextFont.Roboto;

        [ToolTip("Amount of visible digits. 5 Digits is available, if you really need it.")]
        [IntRange(3, 5, 1)]
        public int Digits { get; init; } = 4;

        [IntRange(-10, 30, 1)]
        public int ExtraDigitSpacing { get; init; } = -8;

        [IntRange(1, 100, 1)]
        public int RefreshRate { get; init; } = 30;
    }


    [ConfigGrouping("Colors", "Adjust colors")]
    public ColorsGrouping Colors { get; init; } = new ColorsGrouping();
    public sealed class ColorsGrouping
    {
        public Color TextColor { get; init; } = Color.FromArgb(255, 255, 255, 255);
        [IntRange(75, 255, 1)]
        public int TextOpacity { get; init; } = 255;

        public Color BackgroundColor { get; init; } = Color.FromArgb(255, 0, 0, 0);

        [ToolTip("Changes the background opacity, 0 is invisible.")]
        [IntRange(0, 255, 1)]
        public int BackgroundOpacity { get; init; } = 175;
    }

}