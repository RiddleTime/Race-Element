using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.Telemetry.SharedMemory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.ACCSharedMemory;

namespace ACCSetupApp.Controls.HUD.Overlay
{
    internal class InputTraceOverlay : AbstractOverlay
    {
        private InputDataCollector inputDataCollector;

        public InputTraceOverlay(Rectangle rectangle) : base(rectangle)
        {
            int screenMiddleX = (int)(System.Windows.SystemParameters.FullPrimaryScreenWidth / 2);
            this.X = screenMiddleX + 300;
            this.Y = 0;
            this.Width = 300;
            this.Height = 150;
        }

        public override void BeforeStart()
        {
            inputDataCollector = new InputDataCollector() { TraceCount = this.Width - 1 };
            inputDataCollector.Start();
        }

        public override void BeforeStop()
        {
            inputDataCollector.Stop();
        }

        public override void Render(Graphics g)
        {
            InputGraph graph = new InputGraph(0, 0, this.Width - 1, this.Height - 1, inputDataCollector.Throttle, inputDataCollector.Brake, inputDataCollector.Steering);
            graph.Draw(g);
        }

        public override bool ShouldRender()
        {
            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }
    }
}
