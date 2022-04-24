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
            int width = 140;
            this.X = ScreenWidth - width;

            this.Width = width;
            this.Height = 60 * 2;
            this.Y = (int)(ScreenHeight / 2) - this.Height / 2;
        }

        public override void BeforeStart()
        {
            dataCollector = new TirePressureDataCollector() { TraceCount = this.Width / 2 - 1 };
            dataCollector.Start();
        }

        public override void BeforeStop()
        {
            dataCollector.Stop();
        }

        public override void Render(Graphics g)
        {
            TirePressureGraph graph = new TirePressureGraph(0, 0, this.Width / 2 - 1, (this.Height / 2) - 1, dataCollector.FrontLeft);
            TirePressureGraph graph1 = new TirePressureGraph(this.Width / 2, 0, this.Width / 2 - 1, (this.Height / 2) - 1, dataCollector.FrontRight);
            TirePressureGraph graph2 = new TirePressureGraph(0, (this.Height / 2) * 1, this.Width / 2 - 1, (this.Height / 2) - 1, dataCollector.RearLeft);
            TirePressureGraph graph3 = new TirePressureGraph(this.Width / 2, (this.Height / 2) * 1, this.Width / 2 - 1, (this.Height / 2) - 1, dataCollector.RearRight);

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
