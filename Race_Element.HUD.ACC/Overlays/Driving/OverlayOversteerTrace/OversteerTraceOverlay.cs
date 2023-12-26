using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlaySlipAngle;

[Overlay(Name = "Oversteer Trace", Version = 1.00,
    Description = "Live graph of oversteer in red and understeer in blue.",
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Physics)]
internal sealed class OversteerTraceOverlay : AbstractOverlay
{
    private readonly OversteerTraceConfiguration _config = new();
    internal sealed class OversteerTraceConfiguration : OverlayConfiguration
    {
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

            [ToolTip("Sets the maximum amount of slip angle displayed.")]
            [FloatRange(0.1f, 10f, 0.1f, 1)]
            public float MaxSlipAngle { get; init; } = 1.5f;

            [ToolTip("Sets the data collection rate, this does affect cpu usage at higher values.")]
            [IntRange(10, 100, 5)]
            public int Herz { get; init; } = 30;
        }

        public OversteerTraceConfiguration()
        {
            this.AllowRescale = true;
        }
    }

    private OversteerDataCollector _collector;
    private OversteerGraph _graph;

    public OversteerTraceOverlay(Rectangle rectangle) : base(rectangle, "Oversteer Trace")
    {
        this.Width = _config.Chart.Width;
        this.Height = _config.Chart.Height;
        this.RefreshRateHz = _config.Chart.Herz;
    }

    public sealed override void BeforeStart()
    {
        _collector = new OversteerDataCollector(_config.Chart.Width - 1)
        {
            MaxSlipAngle = _config.Chart.MaxSlipAngle,
            Herz = _config.Chart.Herz
        };
        _graph = new OversteerGraph(0, 0, _config.Chart.Width - 1, _config.Chart.Height - 1, _collector, _config);
    }

    public sealed override void BeforeStop() => _graph.Dispose();

    public sealed override void Render(Graphics g)
    {
        _collector.Collect(pagePhysics);
        _graph.Draw(g);
    }
}
