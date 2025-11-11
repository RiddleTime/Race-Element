using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.InputTrace;

internal sealed class InputTraceConfiguration : OverlayConfiguration
{
    public InputTraceConfiguration() => this.GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Data", "Adjust data collection settings.")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        [ToolTip("Sets the data collection rate.\n70 Hz and higher will affect cpu usage, don't blame us for your cpu.")]
        [IntRange(Min = 10, Max = 150, Increment = 2, GameMaxs = [120], MaxGames = [Game.iRacing])]
        public int Herz { get; init; } = 60;
    }

    [ConfigGrouping("Chart", "Customize the charts refresh rate, data points or hide the steering input.")]
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

        [ToolTip("Displays the steering input as a white line in the trace.")]
        public bool SteeringInput { get; init; } = true;

        [ToolTip("Show horizontal grid lines.")]
        public bool GridLines { get; init; } = true;

        [ToolTip("Sets the drawing refresh rate.")]
        [IntRange(12, 30, 6)]
        public int HudRefreshRate { get; init; } = 30;
    }

    [HideForGame(Game.AssettoCorsa1 | Game.AssettoCorsaEvo | Game.iRacing | Game.AmericanTruckSimulator | Game.EuroTruckSimulator2)]
    [ConfigGrouping("Traction Control", "Adjust settings related to Traction Control Activation.")]
    public TractionControlGrouping TractionControl { get; init; } = new();
    public sealed class TractionControlGrouping
    {
        [ToolTip("Displays Traction Control Activation.")]
        public bool TractionControl { get; init; } = true;

        public Color TractionControlColor { get; init; } = Color.FromArgb(0, 255, 0);
        [IntRange(2, 255, 1)]
        public int TractionControlOpacity { get; init; } = 90;
    }

    [HideForGame(Game.AssettoCorsa1 | Game.AssettoCorsaEvo | Game.AmericanTruckSimulator | Game.EuroTruckSimulator2)]
    [ConfigGrouping("ABS", "Adjust settings related to ABS Activation.")]
    public AbsGrouping Abs { get; init; } = new();
    public sealed class AbsGrouping
    {
        [ToolTip("Displays ABS Activation.")]
        public bool Abs { get; init; } = true;
        public Color AbsColor { get; init; } = Color.FromArgb(255, 0, 0);
        [IntRange(2, 255, 1)]
        public int AbsOpacity { get; init; } = 110;
    }

    [ConfigGrouping("Colors", "Customize the colors of the throttle, brake and steering traces.")]
    public ColorsGrouping Colors { get; init; } = new();
    public sealed class ColorsGrouping
    {
        public Color ThrottleColor { get; init; } = Color.FromArgb(34, 139, 34);
        [IntRange(70, 255, 1)]
        public int ThrottleOpacity { get; init; } = 255;

        public Color BrakeColor { get; init; } = Color.FromArgb(255, 0, 0);
        [IntRange(70, 255, 1)]
        public int BrakeOpacity { get; init; } = 255;

        public Color SteeringColor { get; init; } = Color.FromArgb(255, 255, 255);
        [IntRange(70, 255, 1)]
        public int SteeringOpacity { get; init; } = 190;
    }
}
