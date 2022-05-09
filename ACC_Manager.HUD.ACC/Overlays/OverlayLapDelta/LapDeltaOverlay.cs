using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayLapDelta
{
    internal class LapDeltaOverlay : AbstractOverlay
    {
        private const int overlayWidth = 200;
        private const int overlayHeight = 100;

        InfoPanel panel = new InfoPanel(10, overlayWidth);
        public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Overlay")
        {
            this.Width = overlayWidth + 1;
            this.Height = overlayHeight + 1;
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop()
        {
        }

        public override void Render(Graphics g)
        {
            if (pageGraphics.IsDeltaPositive)
            {
                g.DrawRectangle(Pens.Red, 0, 0, overlayWidth, overlayHeight);
            }
            else
            {
                g.DrawRectangle(Pens.Green, 0, 0, overlayWidth, overlayHeight);
            }

            panel.AddLine("Delta", pageGraphics.DeltaLapTime);
            panel.AddLine("Predicted", pageGraphics.EstimatedLapTime);
            panel.AddLine("Best", pageGraphics.BestTime);
            panel.AddLine("Current", pageGraphics.CurrentTime);

            panel.Draw(g);
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            return false;
        }
    }
}
