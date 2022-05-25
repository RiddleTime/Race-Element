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
            [ToolTip("Displays the time for each sector, green colored sectors are personal best.")]
            public bool ShowSectors { get; set; } = true;

            [ToolTip("Displays the type of the current lap (In/Out/Regular).")]
            public bool ShowLapType { get; set; } = false;

            [ToolTip("Sets the maximum range in seconds for the delta bar.")]
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

        private readonly InfoTable _table = new InfoTable(10, new int[] { 60, 60 });

        public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Overlay")
        {
            overlayHeight = _table.FontHeight * 5;

            this.Width = overlayWidth + 1;
            this.Height = overlayHeight + 1;
            RefreshRateHz = 10;
        }

        public sealed override void BeforeStart()
        {
            if (!this.config.ShowSectors)
                this.Height -= this._table.FontHeight * 3;

            if (!this.config.ShowLapType)
                this.Height -= this._table.FontHeight;

            LapTracker.Instance.LapFinished += Collector_LapFinished;
        }

        public sealed override void BeforeStop()
        {
            LapTracker.Instance.LapFinished -= Collector_LapFinished;
        }

        private void Collector_LapFinished(object sender, LapData newLap)
        {
            if (newLap.Sector1 != -1 && newLap.Sector2 != -1 && newLap.Sector3 != -1)
                lastLap = newLap;
        }

        public sealed override void Render(Graphics g)
        {


            //double delta = (double)pageGraphics.DeltaLapTimeMillis / 1000;
            //panel.AddDeltaBarWithCenteredText($"{delta:F3}", -this.config.MaxDelta, this.config.MaxDelta, delta, true);

            if (this.config.ShowSectors)
                AddSectorLines();

            //if (this.config.ShowLapType)
            //{
            //    string lapType = "Unknown";
            //    if (broadCastRealtimeCarUpdate.CurrentLap != null)
            //        lapType = $"{broadCastRealtimeCarUpdate.CurrentLap.Type}";
            //    panel.AddLine("Type", lapType);
            //}

            _table.Draw(g);
        }

        private void AddSectorLines()
        {
            LapData lap = LapTracker.Instance.CurrentLap;

            if (lastLap != null && pageGraphics.NormalizedCarPosition < 0.08 && lap.Index != lastLap.Index && lastLap.Sector3 != -1)
                lap = lastLap;

            int fastestSector1 = LapTracker.Instance.Laps.GetFastestSector(1);
            int fastestSector2 = LapTracker.Instance.Laps.GetFastestSector(2);
            int fastestSector3 = LapTracker.Instance.Laps.GetFastestSector(3);

            string[] rowSector1 = new string[2];
            string[] rowSector2 = new string[2];
            string[] rowSector3 = new string[2];
            rowSector1[0] = "-";
            rowSector2[0] = "-";
            rowSector3[0] = "-";

            if (LapTracker.Instance.CurrentLap.Sector1 > -1)
            {
                rowSector1[0] = $"{lap.GetSector1():F3}";
                if (lap.Sector1 > fastestSector1)
                    rowSector1[1] = $"+{(float)(lap.Sector1 - fastestSector1) / 1000:F3}";
            }
            else if (pageGraphics.CurrentSectorIndex == 0)
                rowSector1[0] = $"{((float)pageGraphics.CurrentTimeMs / 1000):F3}";


            if (lap.Sector2 > -1)
            {
                rowSector2[0] = $"{lap.GetSector2():F3}";
                if (lap.Sector2 > fastestSector2)
                    rowSector2[1] = $"+{(float)(lap.Sector2 - fastestSector2) / 1000:F3}";
            }
            else if (lap.Sector1 > -1)
            {
                rowSector2[0] = $"{(((float)pageGraphics.CurrentTimeMs - lap.Sector1) / 1000):F3}";
            }

            if (lap.Sector3 > -1)
            {
                rowSector3[0] = $"{lap.GetSector3():F3}";
                if (lap.Sector3 > fastestSector3)
                    rowSector3[1] = $"+{(float)(lap.Sector3 - fastestSector3) / 1000:F3}";
            }
            else if (lap.Sector2 > -1 && pageGraphics.CurrentSectorIndex == 2)
            {
                rowSector3[0] = $"{(((float)pageGraphics.CurrentTimeMs - lap.Sector2 - lap.Sector1) / 1000):F3}";
            }


            if (pageGraphics.CurrentSectorIndex != 0 && lap.Sector1 != -1 && lap.IsValid)
                _table.AddRow("S1", rowSector1, new Color[] { LapTracker.Instance.Laps.IsSectorFastest(1, lap.Sector1) ? Color.LimeGreen : Color.White, Color.Orange });
            else
                _table.AddRow("S1", rowSector1, new Color[] { Color.White });

            if (pageGraphics.CurrentSectorIndex != 1 && lap.Sector2 != -1 && lap.IsValid)
                _table.AddRow("S2", rowSector2, new Color[] { LapTracker.Instance.Laps.IsSectorFastest(2, lap.Sector2) ? Color.LimeGreen : Color.White, Color.Orange });
            else
                _table.AddRow("S2", rowSector2, new Color[] { Color.White });

            if (pageGraphics.CurrentSectorIndex != 2 && lap.Sector3 != -1 && lap.IsValid)
                _table.AddRow("S3", rowSector3, new Color[] { LapTracker.Instance.Laps.IsSectorFastest(3, lap.Sector3) ? Color.LimeGreen : Color.White, Color.Orange });
            else
                _table.AddRow("S3", rowSector3, new Color[] { Color.White });
        }

        public sealed override bool ShouldRender()
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
