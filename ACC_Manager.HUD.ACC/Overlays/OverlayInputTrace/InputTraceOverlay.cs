using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ACCManager.HUD.ACC.Overlays.OverlayInputTrace
{
    internal sealed class InputTraceOverlay : AbstractOverlay
    {
        private readonly InputTraceConfig _config = new InputTraceConfig();
        internal class InputTraceConfig : OverlayConfiguration
        {
            [ToolTip("Displays the steering input as a white line in the trace.")]
            internal bool ShowSteeringInput { get; set; } = true;

            [ToolTip("The amount of datapoints shown, this changes the width of the overlay.")]
            [IntRange(150, 450, 10)]
            internal int DataPoints { get; set; } = 300;

            [ToolTip("Sets the data collection rate.")]
            [IntRange(25, 85, 5)]
            internal int Herz { get; set; } = 50;

            public InputTraceConfig()
            {
                this.AllowRescale = true;
            }
        }

        private readonly int _originalHeight = 150;
        private readonly int _originalWidth = 300;

        private InputDataCollector _inputDataCollector;

        internal static InputTraceOverlay Instance;
        public InputTraceOverlay(Rectangle rectangle) : base(rectangle, "Input Trace Overlay")
        {
            _originalWidth = this._config.DataPoints;
            this.Width = _originalWidth;
            this.Height = _originalHeight;
            this.RequestsDrawItself = true;
            Instance = this;
        }

        public sealed override void BeforeStart()
        {
            _inputDataCollector = new InputDataCollector() { TraceCount = this._originalWidth - 1, inputTraceConfig = _config };
            _inputDataCollector.Start();
        }

        public sealed override void BeforeStop()
        {
            _inputDataCollector.Stop();
        }

        public sealed override void Render(Graphics g)
        {
            InputGraph graph = new InputGraph(0, 0, this._originalWidth - 1, this._originalHeight - 1, _inputDataCollector.Throttle, _inputDataCollector.Brake, _inputDataCollector.Steering, this._config);
            graph.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
