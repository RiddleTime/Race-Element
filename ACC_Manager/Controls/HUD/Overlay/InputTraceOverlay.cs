using ACCSetupApp.Controls.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SharedMemory;

namespace ACCSetupApp.Controls.HUD.Overlay
{
    internal class InputTraceOverlay : AbstractOverlay
    {
        private SharedMemory sharedMemory = new SharedMemory();
        private InputDataCollector inputDataCollector;

        private SPageFilePhysics pagePhysics;
        private SPageFileGraphic pageGraphics;

        public InputTraceOverlay(int x, int y, int width, int height) : base(x, y, width, height)
        {
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
            pagePhysics = sharedMemory.ReadPhysicsPageFile();
            pageGraphics = sharedMemory.ReadGraphicsPageFile();

            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }
    }
}
