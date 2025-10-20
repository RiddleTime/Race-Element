using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.Speedometer;
internal sealed class SpeedometerConfiguration : OverlayConfiguration
{
    public SpeedometerConfiguration() => GenericConfiguration.AllowRescale = false;

    [ConfigGrouping("General", "General options")]
    public GeneralGrouping General { get; init; } = new();
    public sealed class GeneralGrouping
    {
        [ToolTip("Kilometers per Hour or Miles per Hour")]
        public UnitChoice Units { get; init; } = UnitChoice.Kmh;

        [ToolTip("Size of the font")]
        [FloatRange(12.0f, 90.0f, 0.5f, 1)]
        public float FontSize { get; init; } = 25.5f;

        [ToolTip("Change the Font")]
        public RpmTextFont Font { get; init; } = RpmTextFont.Roboto;

        [ToolTip("Amount of visible digits. 4 Digits is available, if you really need it.")]
        [IntRange(3, 4, 1)]
        public int Digits { get; init; } = 3;

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
        [IntRange(75, 255, 1)]
        public int BackgroundOpacity { get; init; } = 175;
    }

    public enum RpmTextFont { Roboto, Conthrax, Obitron, Segoe }
    public enum UnitChoice { Kmh, Mph }
}