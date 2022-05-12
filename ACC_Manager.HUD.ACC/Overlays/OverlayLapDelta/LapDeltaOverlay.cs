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
        private const int overlayWidth = 200;
        private int overlayHeight = 150;

        private LapTimeTracker collector;
        private LapTimingData lastLap = null;

        InfoPanel panel = new InfoPanel(10, overlayWidth);
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
            panel.AddLine("Delta", $"{deltaString}{pageGraphics.DeltaLapTime}");

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
                panel.AddLine("S1", $"{sector1}", IsSectorFastest(1, lap.Sector1) ? Brushes.LimeGreen : Brushes.White);
            else
                panel.AddLine("S1", $"{sector1}");

            if (pageGraphics.CurrentSectorIndex != 1 && lap.Sector2 != -1)
                panel.AddLine("S2", $"{sector2}", IsSectorFastest(2, lap.Sector2) ? Brushes.LimeGreen : Brushes.White);
            else
                panel.AddLine("S2", $"{sector2}");

            if (pageGraphics.CurrentSectorIndex != 2 && lap.Sector3 != -1)
                panel.AddLine("S3", $"{sector3}", IsSectorFastest(3, lap.Sector3) ? Brushes.LimeGreen : Brushes.White);
            else
                panel.AddLine("S3", $"{sector3}");

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="sector">1 based indexing </param>
        /// <param name="lap"></param>
        /// <returns></returns>
        private bool IsSectorFastest(int sector, int time)
        {
            List<LapTimingData> data;
            lock (LapTimeTracker.Instance.LapTimeDatas)
                data = LapTimeTracker.Instance.LapTimeDatas;

            if (sector == 1)
            {
                foreach (LapTimingData timing in data)
                {
                    if (timing.Sector1 < time)
                    {
                        return false;
                    }
                }
            }

            if (sector == 2)
            {
                foreach (LapTimingData timing in data)
                {
                    if (timing.Sector2 < time)
                    {
                        return false;
                    }
                }
            }

            if (sector == 3)
            {
                foreach (LapTimingData timing in data)
                {
                    if (timing.Sector3 < time)
                    {
                        return false;
                    }
                }
            }

            return true;
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
