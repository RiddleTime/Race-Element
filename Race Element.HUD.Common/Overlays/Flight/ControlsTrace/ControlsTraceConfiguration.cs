using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Flight.ControlsTrace;

internal sealed class ControlsTraceConfiguration : OverlayConfiguration
{
    public ControlsTraceConfiguration() => this.GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Data", "Adjust data collection settings.")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        [ToolTip("Sets the data collection rate.\n70 Hz and higher will affect cpu usage, don't blame us for your cpu.")]
        [IntRange(Min = 10, Max = 100, Increment = 2)]
        public int Herz { get; init; } = 60;
    }

    [ConfigGrouping("Chart", "Customize the charts refresh rate, size and style.")]
    public ChartGrouping Chart { get; init; } = new ChartGrouping();
    public sealed class ChartGrouping
    {
        [ToolTip("The amount of datapoints/pixels shown, this changes the width of the overlay.")]
        [IntRange(10, 800, 10)]
        public int Width { get; init; } = 300;

        [ToolTip("The height of the chart, pixel wise.")]
        [IntRange(80, 250, 10)]
        public int Height { get; init; } = 120;

        [ToolTip("Set the thickness of the lines in the chart.")]
        [IntRange(1, 4, 1)]
        public int LineThickness { get; init; } = 2;

        [ToolTip("Show horizontal grid lines.")]
        public bool GridLines { get; init; } = true;

        [ToolTip("Sets the drawing refresh rate.\nA higher refresh rate causes higher cpu usage.")]
        [IntRange(10, 60, 2)]
        public int HudRefreshRate { get; init; } = 30;
    }

    [ConfigGrouping("Colors", "Customize the colors of the Aileron, Elevator and Rudder traces.")]
    public ColorsGrouping Colors { get; init; } = new();
    public sealed class ColorsGrouping
    {
        public Color AileronColor { get; init; } = Color.FromArgb(0, 255, 255);
        [IntRange(70, 255, 1)]
        public int AileronOpacity { get; init; } = 255;

        public Color ElevatorColor { get; init; } = Color.FromArgb(0, 255, 0);
        [IntRange(70, 255, 1)]
        public int ElevatorOpacity { get; init; } = 255;

        public Color RudderColor { get; init; } = Color.FromArgb(255, 165, 0);
        [IntRange(70, 255, 1)]
        public int RudderOpacity { get; init; } = 255;
    }
}
