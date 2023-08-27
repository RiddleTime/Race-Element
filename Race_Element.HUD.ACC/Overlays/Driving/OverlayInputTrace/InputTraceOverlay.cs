using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayInputTrace
{
    [Overlay(Name = "Input Trace", Version = 1.00, OverlayType = OverlayType.Release,
        Description = "Live graph of steering, throttle and brake inputs.",
        OverlayCategory = OverlayCategory.Inputs)]
    internal sealed class InputTraceOverlay : AbstractOverlay
    {
        private readonly InputTraceConfig _config = new InputTraceConfig();
        internal class InputTraceConfig : OverlayConfiguration
        {
            [ConfigGrouping("Chart", "Customize the charts refresh rate, data points or hide the steering input.")]
            public ChartGrouping InfoPanel { get; set; } = new ChartGrouping();
            public class ChartGrouping
            {
                [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
                [IntRange(50, 800, 10)]
                public int Width { get; set; } = 300;

                [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
                [IntRange(80, 250, 10)]
                public int Height { get; set; } = 120;

                [ToolTip("Set the thickness of the lines in the chart.")]
                [IntRange(1, 4, 1)]
                public int LineThickness { get; set; } = 2;

                [ToolTip("Sets the data collection rate, this does affect cpu usage at higher values.")]
                [IntRange(10, 100, 5)]
                public int Herz { get; set; } = 30;

                [ToolTip("Displays the steering input as a white line in the trace.")]
                public bool SteeringInput { get; set; } = true;

                [ToolTip("Show horizontal grid lines.")]
                public bool GridLines { get; set; } = true;
            }

            public InputTraceConfig()
            {
                this.AllowRescale = true;
            }
        }

        private InputGraph _graph;
        private InputDataCollector _inputDataCollector;

        public InputTraceOverlay(Rectangle rectangle) : base(rectangle, "Input Trace")
        {
            this.Width = _config.InfoPanel.Width;
            this.Height = _config.InfoPanel.Height;
            this.RefreshRateHz = _config.InfoPanel.Herz;
        }

        public sealed override void BeforeStart()
        {
            _inputDataCollector = new InputDataCollector(_config.InfoPanel.Width - 1);
            _graph = new InputGraph(0, 0, _config.InfoPanel.Width - 1, _config.InfoPanel.Height - 1, _inputDataCollector, this._config);
        }

        public sealed override void BeforeStop() => _graph?.Dispose();

        public sealed override void Render(Graphics g)
        {
            _inputDataCollector.Collect(pagePhysics);
            _graph.Draw(g);
        }
    }
}
