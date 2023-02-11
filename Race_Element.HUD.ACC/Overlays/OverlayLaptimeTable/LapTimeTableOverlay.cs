using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.OverlayLaptimeTable
{
    [Overlay(Name = "Laptime Table", Description = "A table showing laptimes.", OverlayType = OverlayType.Release, Version = 1.00)]
    internal class LapTimeTableOverlay : AbstractOverlay
    {
        private readonly LapTimeTableConfiguration _config = new LapTimeTableConfiguration();

        private InfoTable _table;
        public LapTimeTableOverlay(Rectangle rectangle) : base(rectangle, "Laptime Table")
        {
            this.Width = 51;
            this.Height = 1;
            this.RefreshRateHz = 3;
        }

        public override void BeforeStart()
        {
            switch (_config.Table.ShowSectors)
            {
                case true:
                    {
                        int[] columnWidths = { 120, 100, 100, 100 };
                        _table = new InfoTable(12, columnWidths);
                        this.Width += columnWidths.Sum();
                        break;
                    }
                case false:
                    {
                        _table = new InfoTable(12, new int[] { 100 });
                        this.Width += 100;
                        break;
                    }
            }
            this.Height += _table.FontHeight * (_config.Table.Rows + 1);
        }

        public override void BeforeStop()
        {
        }

        public override bool ShouldRender() => DefaultShouldRender();

        public override void Render(Graphics g)
        {
            var laps = LapTracker.Instance.Laps.ToList();
            var lapList = laps.OrderByDescending(x => x.Key).Take(_config.Table.Rows);

            switch (_config.Table.ShowSectors)
            {
                case true:
                    {
                        _table.AddRow("#  ", new string[] { "Time", "S1", "S2", "S3" });

                        foreach (var lap in lapList)
                        {
                            string lapTimeValue = $"--:--.---";
                            if (lap.Value.Time != -1)
                            {
                                TimeSpan best = TimeSpan.FromMilliseconds(lap.Value.Time);
                                lapTimeValue = $"{best:mm\\:ss\\:fff}";
                            }

                            string sector1 = $"{lap.Value.GetSector1():F3}";
                            string sector2 = $"{lap.Value.GetSector2():F3}";
                            string sector3 = $"{lap.Value.GetSector2():F3}";

                            _table.AddRow($"{lap.Key}", new string[] { $"{lapTimeValue}", sector1, sector2, sector3 });
                        }
                        break;
                    }
                case false:
                    {
                        _table.AddRow("#  ", new string[] { "Time" });


                        foreach (var lap in lapList)
                        {
                            string lapTimeValue = $"--:--.---";
                            if (lap.Value.Time != -1)
                            {
                                TimeSpan best = TimeSpan.FromMilliseconds(lap.Value.Time);
                                lapTimeValue = $"{best:mm\\:ss\\:fff}";
                            }
                            _table.AddRow($"{lap.Key}", new string[] { $"{lapTimeValue}" });
                        }
                        break;
                    }
            }
            _table?.Draw(g);
        }
    }
}
