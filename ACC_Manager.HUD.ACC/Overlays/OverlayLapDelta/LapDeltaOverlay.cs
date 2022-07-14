using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.ACC.Data.Tracker.Laps;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayLapDelta
{
    [Overlay(Name = "Lap Delta", Version = 1.00,
        Description = "A panel with a bar showing the current delta. Optionally showing the sector times and the potential best.")]
    internal sealed class LapDeltaOverlay : AbstractOverlay
    {
        private readonly LapDeltaConfig _config = new LapDeltaConfig();
        private class LapDeltaConfig : OverlayConfiguration
        {
            [ToolTip("Displays the time for each sector, green colored sectors are personal best.")]
            public bool ShowSectors { get; set; } = true;

            [ToolTip("Displays the potential best lap time based on your fastest sector times.")]
            public bool ShowPotentialBest { get; set; } = true;

            [ToolTip("Sets the maximum range in seconds for the delta bar.")]
            [IntRange(1, 5, 1)]
            public int MaxDelta { get; set; } = 2;

            public LapDeltaConfig() : base()
            {
                this.AllowRescale = true;
            }
        }

        private const int _overlayWidth = 202;
        private readonly InfoTable _table;

        private LapData _lastLap = null;

        public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta Overlay")
        {
            _table = new InfoTable(10, new int[] { 85, 83 }) { Y = 17 };
            this.Width = _overlayWidth + 1;
            this.Height = _table.FontHeight * 5 + 2 + 4;
            RefreshRateHz = 10;
        }

        public sealed override void BeforeStart()
        {
            if (!this._config.ShowSectors)
                this.Height -= this._table.FontHeight * 3;

            if (!this._config.ShowPotentialBest)
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
                _lastLap = newLap;
        }

        public sealed override void Render(Graphics g)
        {
            double delta = (double)pageGraphics.DeltaLapTimeMillis / 1000;
            DeltaBar deltaBar = new DeltaBar(-this._config.MaxDelta, this._config.MaxDelta, delta) { DrawBackground = true };
            deltaBar.Draw(g, 0, 0, _overlayWidth, _table.FontHeight);

            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.TextContrast = 1;

            string deltaText = $"{delta:F3}";
            SizeF textWidth = g.MeasureString(deltaText, _table.Font);
            g.DrawStringWithShadow(deltaText, _table.Font, Color.White, new PointF(_overlayWidth / 2 - textWidth.Width + textWidth.Width / 2, _table.FontHeight / 8f));

            if (this._config.ShowSectors)
                AddSectorLines();

            if (this._config.ShowPotentialBest)
                AddPotentialBest();

            _table.Draw(g);
        }

        private void AddPotentialBest()
        {
            string[] potentialValues = new string[2];

            int potentialBest = LapTracker.Instance.Laps.GetPotentialFastestLapTime();
            if (potentialBest == -1)
                potentialValues[0] = $"--:--.---";
            else
            {
                TimeSpan best = TimeSpan.FromMilliseconds(potentialBest);
                potentialValues[0] = $"{best:mm\\:ss\\:fff}";
            }

            this._table.AddRow("Pot", potentialValues);
        }

        private void AddSectorLines()
        {
            LapData lap = LapTracker.Instance.CurrentLap;

            if (_lastLap != null && pageGraphics.NormalizedCarPosition < 0.08 && lap.Index != _lastLap.Index && _lastLap.Sector3 != -1)
                lap = _lastLap;

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
                _table.AddRow("S1 ", rowSector1, new Color[] { LapTracker.Instance.Laps.IsSectorFastest(1, lap.Sector1) ? Color.LimeGreen : Color.White, Color.Orange });
            else
                _table.AddRow("S1 ", rowSector1, new Color[] { Color.White });

            if (pageGraphics.CurrentSectorIndex != 1 && lap.Sector2 != -1 && lap.IsValid)
                _table.AddRow("S2 ", rowSector2, new Color[] { LapTracker.Instance.Laps.IsSectorFastest(2, lap.Sector2) ? Color.LimeGreen : Color.White, Color.Orange });
            else
                _table.AddRow("S2 ", rowSector2, new Color[] { Color.White });

            if (pageGraphics.CurrentSectorIndex != 2 && lap.Sector3 != -1 && lap.IsValid)
                _table.AddRow("S3 ", rowSector3, new Color[] { LapTracker.Instance.Laps.IsSectorFastest(3, lap.Sector3) ? Color.LimeGreen : Color.White, Color.Orange });
            else
                _table.AddRow("S3 ", rowSector3, new Color[] { Color.White });
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
