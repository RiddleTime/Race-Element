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
        private const int overlayHeight = 150;

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


            string sector1 = "-";
            string sector2 = "-";
            string sector3 = "-";
            if (collector.Sectors1.Count > 0) sector1 = $"{((float)collector.Sectors1.Last().Value / 1000):F3}";
            if (collector.Sectors2.Count > 0) sector2 = $"{((float)collector.Sectors2.Last().Value / 1000):F3}";
            if (collector.Sectors3.Count > 0) sector3 = $"{((float)collector.Sectors3.Last().Value / 1000):F3}";
            panel.AddLine("S1", $"{sector1}", IsLastSectorFastest(collector.Sectors1) ? Brushes.LimeGreen : Brushes.White);
            panel.AddLine("S2", $"{sector2}", IsLastSectorFastest(collector.Sectors2) ? Brushes.LimeGreen : Brushes.White);
            panel.AddLine("S3", $"{sector3}", IsLastSectorFastest(collector.Sectors3) ? Brushes.LimeGreen : Brushes.White);
            panel.AddLine("Sector", $"{pageGraphics.CurrentSectorIndex}");
            panel.AddLine("collected?", $"{collector.LapTimes.Count}");
            panel.Draw(g);


            Pen isbetterPen = Pens.Green;
            if (pageGraphics.IsDeltaPositive || !pageGraphics.IsValidLap)
                isbetterPen = Pens.Red;

            g.DrawRoundedRectangle(isbetterPen, new Rectangle(0, 0, overlayWidth, overlayHeight), 3);
        }




        public bool IsLastSectorFastest(Dictionary<int, int> sectorTimes)
        {
            if (sectorTimes.Count == 0) return false;
            if (!pageGraphics.IsValidLap) return false;

            int sectorTime = sectorTimes.Last().Value;
            if (sectorTime < 0) { return false; }

            foreach (KeyValuePair<int, int> kvp in sectorTimes)
            {
                if (collector.LapValids.ContainsKey(kvp.Key))
                    if (collector.LapValids[kvp.Key])
                        if (sectorTime > kvp.Value)
                            return false;
            }

            return true;
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
