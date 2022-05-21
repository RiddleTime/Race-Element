using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Util;
using ACCManager.Controls.Telemetry.SharedMemory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayInputTrace
{
    internal sealed class InputTraceOverlay : AbstractOverlay
    {
        private InputTraceConfig config = new InputTraceConfig();
        internal class InputTraceConfig : OverlayConfiguration
        {
            internal bool ShowSteeringInput { get; set; } = true;
            public InputTraceConfig()
            {
                this.AllowRescale = true;
            }
        }

        private int OriginalHeight = 150;
        private int OriginalWidth = 300;

        private InputDataCollector inputDataCollector;

        public InputTraceOverlay(Rectangle rectangle) : base(rectangle, "Input Trace Overlay")
        {
            this.Width = OriginalWidth;
            this.Height = OriginalHeight;
        }

        public override void BeforeStart()
        {
            inputDataCollector = new InputDataCollector() { TraceCount = this.OriginalWidth - 1 };
            inputDataCollector.Start();
        }

        public override void BeforeStop()
        {
            inputDataCollector.Stop();
        }

        public override void Render(Graphics g)
        {
            InputGraph graph = new InputGraph(0, 0, this.OriginalWidth - 1, this.OriginalHeight - 1, inputDataCollector.Throttle, inputDataCollector.Brake, inputDataCollector.Steering, this.config);
            graph.Draw(g);
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }
    }
}
