using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.InputTrace;

internal sealed class InputTraceConfiguration : OverlayConfiguration
{
    public InputTraceConfiguration() => this.GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Data", "Adjust data collection settings.")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        [ToolTip("Sets the data collection rate.")]
        [IntRange(10, 100, 2)]
        public int Herz { get; init; } = 70;
    }

    [ConfigGrouping("Chart", "Customize the charts refresh rate, data points or hide the steering input.")]
    public ChartGrouping Chart { get; init; } = new ChartGrouping();
    public sealed class ChartGrouping
    {
        [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
        [IntRange(10, 800, 10)]
        public int Width { get; init; } = 300;

        [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
        [IntRange(80, 250, 10)]
        public int Height { get; init; } = 120;

        [ToolTip("Set the thickness of the lines in the chart.")]
        [IntRange(1, 4, 1)]
        public int LineThickness { get; init; } = 1;

        [ToolTip("Displays the steering input as a white line in the trace.")]
        public bool SteeringInput { get; init; } = true;

        [ToolTip("Show horizontal grid lines.")]
        public bool GridLines { get; init; } = true;

        [ToolTip("Sets the drawing refresh rate.")]
        [IntRange(12, 30, 6)]
        public int HudRefreshRate { get; init; } = 24;
    }
}
