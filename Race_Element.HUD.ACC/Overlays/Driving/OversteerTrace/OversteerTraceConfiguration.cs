using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.OversteerTrace;
internal sealed class OversteerTraceConfiguration : OverlayConfiguration
{
    public OversteerTraceConfiguration() => this.GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Data", "Adjust data bounds.")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        [ToolTip("Sets the maximum amount of slip angle displayed.")]
        [FloatRange(0.1f, 10f, 0.1f, 1)]
        public float MaxSlipAngle { get; init; } = 1.5f;

        [ToolTip("Sets the data collection rate, this does affect cpu usage at higher values.")]
        [IntRange(10, 100, 2)]
        public int Herz { get; init; } = 70;
    }

    [ConfigGrouping("Chart", "Customize the charts refresh rate, data points or change the max slip angle shown.")]
    public ChartGrouping Chart { get; init; } = new ChartGrouping();
    public class ChartGrouping
    {
        [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
        [IntRange(50, 800, 10)]
        public int Width { get; init; } = 300;

        [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
        [IntRange(80, 250, 10)]
        public int Height { get; init; } = 120;

        [ToolTip("Set the thickness of the lines in the chart.")]
        [IntRange(1, 4, 1)]
        public int LineThickness { get; init; } = 1;
    }
}
