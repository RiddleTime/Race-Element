using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
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

        private LapTimeCollector collector;

        InfoPanel panel = new InfoPanel(10, overlayWidth);
        public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Overlay")
        {
            this.Width = overlayWidth + 1;
            this.Height = overlayHeight + 1;
            RefreshRateHz = 10;
        }

        public override void BeforeStart()
        {
            collector = new LapTimeCollector();
            collector.Start();
        }

        public override void BeforeStop()
        {
            collector.Stop();
        }

        public override void Render(Graphics g)
        {
            string deltaString = pageGraphics.IsDeltaPositive ? "+" : "-";
            panel.AddLine("Delta", $"{deltaString}{pageGraphics.DeltaLapTime}");
            panel.AddLine("Predicted", pageGraphics.EstimatedLapTime);
            panel.AddLine("Best", pageGraphics.BestTime);
            panel.AddLine("Current", pageGraphics.CurrentTime);
            panel.AddLine("sector i", $"{pageGraphics.CurrentSectorIndex}");
            panel.AddLine("Sector", $"{((float)pageGraphics.LastSectorTime / 1000):F3}");
            panel.AddLine("Valid?", $"{pageGraphics.IsValidLap}");
            panel.Draw(g);


            Pen isbetterPen = Pens.Green;
            if (pageGraphics.IsDeltaPositive || !pageGraphics.IsValidLap)
                isbetterPen = Pens.Red;

            g.DrawRoundedRectangle(isbetterPen, new Rectangle(0, 0, overlayWidth, overlayHeight), 3);
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
