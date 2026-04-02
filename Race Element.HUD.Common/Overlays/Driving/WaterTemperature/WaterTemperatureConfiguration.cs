using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.WaterTemperature;

internal sealed class WaterTemperatureConfiguration : OverlayConfiguration
{
    public WaterTemperatureConfiguration() => GenericConfiguration.AllowRescale = false;

    [ConfigGrouping("General", "General options")]
    public GeneralGrouping General { get; init; } = new();
    public sealed class GeneralGrouping
    {
        [ToolTip("Display temperature in Celsius or Fahrenheit")]
        public UnitChoice Units { get; init; } = UnitChoice.Celsius;

        [ToolTip("Size of the font")]
        [FloatRange(12.0f, 90.0f, 0.5f, 1)]
        public float FontSize { get; init; } = 25.5f;

        [ToolTip("Change the Font")]
        public TemperatureTextFont Font { get; init; } = TemperatureTextFont.Roboto;

        [ToolTip("Number of digits to display")]
        [IntRange(2, 3, 1)]
        public int Digits { get; init; } = 3;

        [IntRange(-10, 30, 1)]
        public int ExtraDigitSpacing { get; init; } = -8;

        [IntRange(1, 100, 1)]
        public int RefreshRate { get; init; } = 10;

        [ToolTip("Show the unit symbol (°C or °F)")]
        public bool ShowUnit { get; init; } = true;

        [ToolTip("Show label text")]
        public bool ShowLabel { get; init; } = true;

        [ToolTip("Label text to display")]
        public string LabelText { get; init; } = "H₂O";
    }

    [ConfigGrouping("Colors", "Adjust colors")]
    public ColorsGrouping Colors { get; init; } = new();
    public sealed class ColorsGrouping
    {
        public Color TextColor { get; init; } = Color.FromArgb(255, 255, 255, 255);
        
        [IntRange(75, 255, 1)]
        public int TextOpacity { get; init; } = 255;

        public Color BackgroundColor { get; init; } = Color.FromArgb(255, 0, 0, 0);

        [ToolTip("Changes the background opacity, 0 is invisible.")]
        [IntRange(0, 255, 1)]
        public int BackgroundOpacity { get; init; } = 175;

        [ToolTip("Color when temperature is in normal range")]
        public Color NormalColor { get; init; } = Color.FromArgb(255, 100, 200, 100);

        [ToolTip("Color when temperature is getting hot")]
        public Color WarningColor { get; init; } = Color.FromArgb(255, 255, 200, 0);

        [ToolTip("Color when temperature is critical")]
        public Color DangerColor { get; init; } = Color.FromArgb(255, 255, 50, 50);
    }

    [ConfigGrouping("Warning Thresholds", "Temperature warning thresholds")]
    public ThresholdsGrouping Thresholds { get; init; } = new();
    public sealed class ThresholdsGrouping
    {
        [ToolTip("Temperature (°C) above which warning color is used")]
        [IntRange(70, 120, 1)]
        public int WarningTemperature { get; init; } = 95;

        [ToolTip("Temperature (°C) above which danger color is used")]
        [IntRange(80, 130, 1)]
        public int DangerTemperature { get; init; } = 105;
    }

    public enum TemperatureTextFont { Roboto, Conthrax, Obitron, Segoe }
    public enum UnitChoice { Celsius, Fahrenheit }
}
