using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using System.Drawing;

namespace ACCManager.HUD.ACC.Overlays.OverlaySlipAngle
{
    [Overlay(Name = "Oversteer Trace", Version = 1.00,
        Description = "Live graph of oversteer in red and understeer in blue.", OverlayType = OverlayType.Release)]
    internal sealed class OversteerTraceOverlay : AbstractOverlay
    {
        private readonly OversteerTraceConfiguration _config = new OversteerTraceConfiguration();
        private class OversteerTraceConfiguration : OverlayConfiguration
        {
            [ToolTip("Sets the maximum amount of slip angle displayed.")]
            [IntRange(1, 90, 1)]
            public int MaxSlipAngle { get; set; } = 4;

            [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
            [IntRange(150, 800, 10)]
            public int DataPoints { get; set; } = 300;

            [ToolTip("Sets the data collection rate, this does affect cpu usage at higher values.")]
            [IntRange(10, 70, 5)]
            internal int Herz { get; set; } = 40;

            public OversteerTraceConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private OversteerDataCollector _collector;
        private OversteerGraph _graph;
        private readonly int _originalWidth;
        private readonly int _originalHeight = 120;

        public OversteerTraceOverlay(Rectangle rectangle) : base(rectangle, "Oversteer Trace Overlay")
        {
            _originalWidth = this._config.DataPoints;

            this.Width = _originalWidth;
            this.Height = _originalHeight;
            this.RequestsDrawItself = true;
        }

        public sealed override void BeforeStart()
        {
            _collector = new OversteerDataCollector(this)
            {
                TraceCount = _originalWidth - 1,
                MaxSlipAngle = _config.MaxSlipAngle,
                Herz = _config.Herz
            };
            _collector.Start();

            _graph = new OversteerGraph(0, 0, _originalWidth - 1, _originalHeight - 1, _collector);
        }

        public sealed override void BeforeStop()
        {
            _collector.Stop();
            _graph.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            _graph.Draw(g);
        }

        public sealed override bool ShouldRender() => DefaultShouldRender();
    }
}
