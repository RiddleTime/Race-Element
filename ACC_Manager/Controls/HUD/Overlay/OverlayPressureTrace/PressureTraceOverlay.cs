using ACCSetupApp.Controls.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.ACCSharedMemory;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayPressureTrace
{
    internal class PressureTraceOverlay : AbstractOverlay
    {
        private TirePressureDataCollector dataCollector;
        public PressureTraceOverlay(Rectangle rectangle) : base(rectangle)
        {
            this.X = 0;
            this.Y = 0;
            this.Width = 300;
            this.Height = 80 * 4;
        }

        public override void BeforeStart()
        {
            dataCollector = new TirePressureDataCollector() { TraceCount = this.Width - 1 };
            dataCollector.Start();
        }

        public override void BeforeStop()
        {
            dataCollector.Stop();
        }

        public override void Render(Graphics g)
        {
            TirePressureGraph graph = new TirePressureGraph(0, 0, this.Width - 1, (this.Height / 4) - 1, dataCollector.FrontLeft);
            TirePressureGraph graph1 = new TirePressureGraph(0, (this.Height / 4) * 1, this.Width - 1, (this.Height / 4) - 1, dataCollector.FrontRight);
            TirePressureGraph graph2 = new TirePressureGraph(0, (this.Height / 4) * 2, this.Width - 1, (this.Height / 4) - 1, dataCollector.RearLeft);
            TirePressureGraph graph3 = new TirePressureGraph(0, (this.Height / 4) * 3, this.Width - 1, (this.Height / 4) - 1, dataCollector.RearRight);

            graph.Draw(g);
            graph1.Draw(g);
            graph2.Draw(g);
            graph3.Draw(g);
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
