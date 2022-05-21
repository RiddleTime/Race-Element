using ACCManager.HUD.ACC.Data.Tracker;
using ACCManager.HUD.ACC.Data.Tracker.Laps;
using ACCManager.HUD.Overlay.Configuration;
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
    internal sealed class LapDeltaOverlay : AbstractOverlay
    {
        private readonly LapDeltaConfig config = new LapDeltaConfig();
        private class LapDeltaConfig : OverlayConfiguration
        {
            public bool ShowSectors { get; set; } = true;
            public bool ShowLapType { get; set; } = false;

            [IntRange(1, 5, 1)]
            public int MaxDelta { get; set; } = 2;

            public LapDeltaConfig() : base()
            {
                this.AllowRescale = true;
            }
        }

        private const int overlayWidth = 200;
        private int overlayHeight = 150;

        private LapData lastLap = null;

        InfoPanel panel = new InfoPanel(11, overlayWidth);
        public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Overlay")
        {
            overlayHeight = panel.FontHeight * 5;

            this.Width = overlayWidth + 1;
            this.Height = overlayHeight + 1;
            RefreshRateHz = 10;
        }

        public override void BeforeStart()
        {
            if (!this.config.ShowSectors)
                this.Height -= this.panel.FontHeight * 3;

            if (!this.config.ShowLapType)
                this.Height -= this.panel.FontHeight;

            LapTracker.Instance.LapFinished += Collector_LapFinished;
        }

        public override void BeforeStop()
        {
            LapTracker.Instance.LapFinished -= Collector_LapFinished;
        }

        private void Collector_LapFinished(object sender, LapData newLap)
        {
            if (newLap.Sector1 != -1 && newLap.Sector2 != -1 && newLap.Sector3 != -1)
                lastLap = newLap;
        }

        public override void Render(Graphics g)
        {
            double delta = (double)pageGraphics.DeltaLapTimeMillis / 1000;
            panel.AddDeltaBarWithCenteredText($"{delta:F3}", -this.config.MaxDelta, this.config.MaxDelta, delta);

            if (this.config.ShowSectors)
                AddSectorLines();

            if (this.config.ShowLapType)
            {
                string lapType = "Unknown";
                if (broadCastRealtimeCarUpdate.CurrentLap != null)
                    lapType = $"{broadCastRealtimeCarUpdate.CurrentLap.Type}";
                panel.AddLine("Type", lapType);
            }

            panel.Draw(g);
        }

        private void AddSectorLines()
        {
            LapData lap = LapTracker.Instance.CurrentLap;

            if (lastLap != null && pageGraphics.NormalizedCarPosition < 0.08)
                lap = lastLap;

            string sector1 = "-";
            string sector2 = "-";
            string sector3 = "-";
            if (LapTracker.Instance.CurrentLap.Sector1 > -1)
            {
                sector1 = $"{lap.GetSector1():F3}";
            }
            else if (pageGraphics.CurrentSectorIndex == 0)
                sector1 = $"{((float)pageGraphics.CurrentTimeMs / 1000):F3}";


            if (lap.Sector2 > -1)
                sector2 = $"{lap.GetSector2():F3}";
            else if (lap.Sector1 > -1)
            {
                sector2 = $"{(((float)pageGraphics.CurrentTimeMs - lap.Sector1) / 1000):F3}";
            }

            if (lap.Sector3 > -1)
                sector3 = $"{lap.GetSector3():F3}";
            else if (lap.Sector2 > -1 && pageGraphics.CurrentSectorIndex == 2)
            {
                sector3 = $"{(((float)pageGraphics.CurrentTimeMs - lap.Sector2 - lap.Sector1) / 1000):F3}";
            }


            if (pageGraphics.CurrentSectorIndex != 0 && lap.Sector1 != -1 && lap.IsValid)
                panel.AddLine("S1", $"{sector1}", LapTracker.Instance.Laps.IsSectorFastest(1, lap.Sector1) ? Brushes.LimeGreen : Brushes.White);
            else
                panel.AddLine("S1", $"{sector1}");

            if (pageGraphics.CurrentSectorIndex != 1 && lap.Sector2 != -1 && lap.IsValid)
                panel.AddLine("S2", $"{sector2}", LapTracker.Instance.Laps.IsSectorFastest(2, lap.Sector2) ? Brushes.LimeGreen : Brushes.White);
            else
                panel.AddLine("S2", $"{sector2}");

            if (pageGraphics.CurrentSectorIndex != 2 && lap.Sector3 != -1 && lap.IsValid)
                panel.AddLine("S3", $"{sector3}", LapTracker.Instance.Laps.IsSectorFastest(3, lap.Sector3) ? Brushes.LimeGreen : Brushes.White);
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
