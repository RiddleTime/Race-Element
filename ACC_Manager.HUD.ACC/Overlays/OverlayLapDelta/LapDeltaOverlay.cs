using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayLapDelta
{
    internal class LapDeltaOverlay : AbstractOverlay
    {
        private LapDeltaConfig config = new LapDeltaConfig();
        private class LapDeltaConfig : OverlayConfiguration
        {
            public LapDeltaConfig() : base()
            {
                this.AllowRescale = true;
            }
        }

        private const int overlayWidth = 200;
        private int overlayHeight = 150;

        private LapTimeTracker collector;
        private LapTimingData lastLap = null;

        InfoPanel panel = new InfoPanel(11, overlayWidth);
        public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Overlay")
        {
            overlayHeight = panel.FontHeight * 4;

            this.Width = overlayWidth + 1;
            this.Height = overlayHeight + 1;
            RefreshRateHz = 10;
        }

        public override void BeforeStart()
        {
            collector = LapTimeTracker.Instance;
            collector.Start();
            collector.LapFinished += Collector_LapFinished;
        }

        public override void BeforeStop()
        {
            collector.LapFinished -= Collector_LapFinished;
            collector.Stop();
        }

        private void Collector_LapFinished(object sender, LapTimingData e)
        {
            lastLap = e;
        }

        public override void Render(Graphics g)
        {
            string deltaString = pageGraphics.IsDeltaPositive ? "+" : "-";
            double delta = (double)pageGraphics.DeltaLapTimeMillis / 1000;
            panel.AddDeltaBarWithCenteredText($"{delta:F3}", -1, 1, delta);

            AddSectorLines();

            panel.Draw(g);


            Pen isbetterPen = Pens.Green;
            if (pageGraphics.IsDeltaPositive || !pageGraphics.IsValidLap)
                isbetterPen = Pens.Red;

            g.DrawRoundedRectangle(isbetterPen, new Rectangle(0, 0, overlayWidth, overlayHeight), 3);
        }

        private void AddSectorLines()
        {
            LapTimingData lap = collector.CurrentLap;

            if (lastLap != null && pageGraphics.NormalizedCarPosition < 0.08)
            {
                lap = lastLap;
            }

            string sector1 = "-";
            string sector2 = "-";
            string sector3 = "-";
            if (collector.CurrentLap.Sector1 > -1)
            {
                sector1 = $"{((float)lap.Sector1 / 1000):F3}";
            }
            else if (pageGraphics.CurrentSectorIndex == 0)
                sector1 = $"{((float)pageGraphics.CurrentTimeMs / 1000):F3}";


            if (lap.Sector2 > -1)
                sector2 = $"{((float)lap.Sector2 / 1000):F3}";
            else if (lap.Sector1 > -1)
            {
                sector2 = $"{(((float)pageGraphics.CurrentTimeMs - lap.Sector1) / 1000):F3}";
            }

            if (lap.Sector3 > -1)
                sector3 = $"{((float)lap.Sector3 / 1000):F3}";
            else if (lap.Sector2 > -1 && pageGraphics.CurrentSectorIndex == 2)
            {
                sector3 = $"{(((float)pageGraphics.CurrentTimeMs - lap.Sector2 - lap.Sector1) / 1000):F3}";
            }

            if (pageGraphics.CurrentSectorIndex != 0 && lap.Sector1 != -1)
                panel.AddLine("S1", $"{sector1}", collector.IsSectorFastest(1, lap.Sector1) ? Brushes.LimeGreen : Brushes.White);
            else
                panel.AddLine("S1", $"{sector1}");

            if (pageGraphics.CurrentSectorIndex != 1 && lap.Sector2 != -1)
                panel.AddLine("S2", $"{sector2}", collector.IsSectorFastest(2, lap.Sector2) ? Brushes.LimeGreen : Brushes.White);
            else
                panel.AddLine("S2", $"{sector2}");

            if (pageGraphics.CurrentSectorIndex != 2 && lap.Sector3 != -1)
                panel.AddLine("S3", $"{sector3}", collector.IsSectorFastest(3, lap.Sector3) ? Brushes.LimeGreen : Brushes.White);
            else
                panel.AddLine("S3", $"{sector3}");
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
