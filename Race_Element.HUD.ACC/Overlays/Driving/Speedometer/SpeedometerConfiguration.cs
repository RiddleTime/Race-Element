using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlaySpeedometer;

internal sealed class SpeedometerConfiguration : OverlayConfiguration
{
    public enum GuageDirection { Clockwise, CounterClockwise }

    public SpeedometerConfiguration() => GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Settings", "Adjust general settings for the Speedometer overlay.")]
    public SettingsGrouping Settings { get; init; } = new SettingsGrouping();
    public sealed class SettingsGrouping
    {
        [ToolTip("Sets the maximum speed used as the percentage of the gauge (300 default)")]
        [IntRange(250, 340, 1)]
        public int MaxSpeed { get; init; } = 300;

        [ToolTip("Sets the refreshrate")]
        [IntRange(10, 50, 2)]
        public int RefreshRate { get; init; } = 20;
    }

    [ConfigGrouping("Shape", "Modify the shape of the gauge.")]
    public ShapeGrouping Shape { get; init; } = new ShapeGrouping();
    public sealed class ShapeGrouping
    {
        [ToolTip("Sets the way the gauge fills up, Clockwise is positive degrees and counter clockwise is negative degrees.")]
        public GuageDirection Direction { get; init; } = GuageDirection.Clockwise;

        [ToolTip("Sets where the circular gauge starts (0 degrees is at the positive x-axis, 135 is default).")]
        [IntRange(0, 360, 1)]
        public int StartAngle { get; init; } = 135;

        [ToolTip("Sets the amount of degrees of the circular gauge (270 is defualt).")]
        [IntRange(45, 340, 1)]
        public int SweepAngle { get; init; } = 270;
    }

    [ConfigGrouping("Colors", "Modify the colors of the speedometer.")]
    public ColorGrouping Colors { get; init; } = new ColorGrouping();
    public sealed class ColorGrouping
    {
        [ToolTip("Sets the color of the speed text")]
        public Color TextColor { get; init; } = Color.FromArgb(255, 255, 255, 255);

        [ToolTip("Sets the color of indicator that shows the percentual RPMs.")]
        public Color IndicatorColor { get; init; } = Color.FromArgb(255, 255, 255, 255);

        [ToolTip("Sets the background color of the gauge")]
        public Color BackgroundColor { get; init; } = Color.FromArgb(255, 0, 0, 0);

        [ToolTip("Sets the opacity of the background color")]
        [IntRange(50, 255, 1)]
        public int BackgroundOpacity { get; init; } = 170;
    }
}
