using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

internal sealed class GForceTraceConfiguration : OverlayConfiguration
{
    public GForceTraceConfiguration() => GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Data", "Adjust data bounds.")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        [FloatRange(1, 3, 0.1f, 1)]
        public float MaxLatG { get; init; } = 3f;

        [FloatRange(1, 3, 0.1f, 1)]
        public float MaxLongG { get; init; } = 2.5f;
    }

    [ConfigGrouping("Chart", "Customize the charts refresh rate or amount of data points.")]
    public ChartGrouping Chart { get; init; } = new ChartGrouping();
    public class ChartGrouping
    {
        [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
        [IntRange(10, 800, 10)]
        public int Width { get; init; } = 300;

        [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
        [IntRange(80, 250, 10)]
        public int Height { get; init; } = 120;

        [ToolTip("Set the thickness of the lines in the chart.")]
        [IntRange(1, 4, 1)]
        public int LineThickness { get; init; } = 2;

        [ToolTip("Sets the data collection rate, this does affect cpu usage at higher values.")]
        [IntRange(10, 70, 5)]
        public int Herz { get; init; } = 30;


        [ToolTip("Show horizontal grid lines.")]
        public bool GridLines { get; init; } = true;
    }
}

