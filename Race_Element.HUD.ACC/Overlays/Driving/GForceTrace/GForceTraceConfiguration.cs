using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

internal sealed class GForceTraceConfiguration : OverlayConfiguration
{
    public GForceTraceConfiguration() => GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Data", "Adjust data bounds.")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        [ToolTip("Sets the maximum lateral g-force displayed.")]
        [FloatRange(1, 3, 0.1f, 1)]
        public float MaxLatG { get; init; } = 3f;

        [ToolTip("Sets the maximum longitudinal g-force displayed.")]
        [FloatRange(1, 3, 0.1f, 1)]
        public float MaxLongG { get; init; } = 2.5f;

        [ToolTip("Sets the data collection rate.")]
        [IntRange(10, 100, 2)]
        public int Herz { get; init; } = 70;
    }

    [ConfigGrouping("Chart", "Customize the appearance of the live trace.")]
    public ChartGrouping Chart { get; init; } = new ChartGrouping();
    public sealed class ChartGrouping
    {
        [ToolTip("The amount of datapoints shown, this changes the width of the chart.")]
        [IntRange(10, 800, 10)]
        public int Width { get; init; } = 300;

        [ToolTip("The height of the chart.")]
        [IntRange(80, 250, 10)]
        public int Height { get; init; } = 120;

        [ToolTip("Set the thickness of the lines in the chart.")]
        [IntRange(1, 4, 1)]
        public int LineThickness { get; init; } = 2;

        [ToolTip("Show horizontal grid lines.")]
        public bool GridLines { get; init; } = true;

        [ToolTip("Sets the drawing refresh rate.")]
        [IntRange(12, 30, 6)]
        public int HudRefreshRate { get; init; } = 24;
    }
}

