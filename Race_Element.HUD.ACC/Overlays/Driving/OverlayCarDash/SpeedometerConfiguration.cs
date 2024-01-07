using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCarDash;

internal sealed class SpeedometerConfiguration : OverlayConfiguration
{
    public enum GuageDirection { Clockwise, CounterClockwise }

    public SpeedometerConfiguration() => AllowRescale = true;

    [ConfigGrouping("Settings", "Adjust general settings for the Speedometer overlay.")]
    public SettingsGrouping Settings { get; init; } = new SettingsGrouping();
    public sealed class SettingsGrouping
    {
        [IntRange(250, 340, 1)]
        public int MaxSpeed { get; init; } = 300;
    }

    [ConfigGrouping("Shape", "Adjust the shape of the rpm indicator.")]
    public ShapeGrouping Shape { get; init; } = new ShapeGrouping();
    public sealed class ShapeGrouping
    {
        [ToolTip("Adjust the way the guage fills up, Clockwise is positive degrees and counter clockwise is negative degrees.")]
        public GuageDirection Direction { get; init; } = GuageDirection.Clockwise;

        [ToolTip("Adjust where the circular guage starts (0 degrees is at the positive x-xis).")]
        [IntRange(0, 360, 1)]
        public int StartAngle { get; init; } = 120;

        [ToolTip("Adjust the amount of degrees of the circular guage.")]
        [IntRange(45, 360, 1)]
        public int SweepAngle { get; init; } = 240;
    }
}
