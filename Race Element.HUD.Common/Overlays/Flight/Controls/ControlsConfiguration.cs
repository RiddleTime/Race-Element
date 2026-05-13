using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Flight.Controls;

internal sealed class ControlsConfiguration : OverlayConfiguration
{
    public ControlsConfiguration() => this.GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Joystick Indicator", "Adjust colors and line thickness for the JoyStick indicator.")]
    public JoyStickIndicatorGrouping JoyStickIndicator { get; init; } = new();
    public sealed class JoyStickIndicatorGrouping
    {
        [ToolTip("Set to 0 for not drawing a line")]
        [IntRange(0, 6, 1)]
        public int LineThickness { get; init; } = 4;
        public Color LineColor { get; init; } = Color.FromArgb(255, 255, 255);
        [IntRange(0, 255, 1)]
        public int LineOpacity { get; init; } = 240;
        public Color IndicatorFillColor { get; init; } = Color.FromArgb(0, 180, 255);
        [IntRange(0, 255, 1)]
        public int IndicatorFillOpacity { get; init; } = 255;

        public Color IndicatorBorderColor { get; init; } = Color.FromArgb(255, 255, 255);
        [IntRange(0, 255, 1)]
        public int IndicatorBorderOpacity { get; init; } = 255;
    }

    [ConfigGrouping("Joystick Indicator Background", "Adjust the background for the JoyStick indicator.")]
    public JoyStickBackgroundGrouping JoyStickBackground { get; init; } = new();
    public sealed class JoyStickBackgroundGrouping
    {
        public bool DrawCrosshair { get; init; } = true;
        public Color FillColor { get; init; } = Color.FromArgb(55, 55, 55);
        [IntRange(0, 255, 1)]
        public int FillOpacity { get; init; } = 255;
        public Color BorderColor { get; init; } = Color.FromArgb(110, 110, 110);
        [IntRange(0, 255, 1)]
        public int BorderOpacity { get; init; } = 255;
    }

    [ConfigGrouping("Rudder Bar", "Adjust the Horizontal Rudder bar.")]
    public RudderBarGrouping RudderBar { get; init; } = new();
    public sealed class RudderBarGrouping
    {
        public Color RudderColor { get; init; } = Color.FromArgb(255, 164, 40);
        [IntRange(0, 255, 1)]
        public int RudderOpacity { get; init; } = 255;
        public Color BackgroundColor { get; init; } = Color.FromArgb(55, 55, 55);
        [IntRange(0, 255, 1)]
        public int BackGroundOpacity { get; init; } = 255;
        public Color BorderColor { get; init; } = Color.FromArgb(200, 200, 200);
        [IntRange(0, 255, 1)]
        public int BorderOpacity { get; init; } = 120;
    }

    [ConfigGrouping("Main Background Colors", "Adjust the Main Background colors and opacity.")]
    public MainBackgroundGrouping MainBackground { get; init; } = new();
    public sealed class MainBackgroundGrouping
    {
        public Color FillColor { get; init; } = Color.FromArgb(0, 0, 0);
        [IntRange(0, 255, 1)]
        public int FillOpacity { get; init; } = 170;

        public Color BorderColor { get; init; } = Color.FromArgb(0, 0, 0);
        [IntRange(0, 255, 1)]
        public int BorderOpacity { get; init; } = 170;
    }

}
