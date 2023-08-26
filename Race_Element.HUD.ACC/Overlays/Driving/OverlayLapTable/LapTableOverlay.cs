using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Drawing;
using System.Linq;
using static RaceElement.HUD.Overlay.OverlayUtil.InfoTable;

namespace RaceElement.HUD.ACC.Overlays.OverlayLapTimeTable
{
    [Overlay(Name = "Lap Table", Description = "A table showing time for each lap and optionally sectors.", OverlayType = OverlayType.Release, Version = 1.00,
        OverlayCategory = OverlayCategory.Lap)]
    internal sealed class LapTableOverlay : AbstractOverlay
    {
        private readonly LapTimeTableConfiguration _config = new LapTimeTableConfiguration();

        private InfoTable _table;
        public LapTableOverlay(Rectangle rectangle) : base(rectangle, "Lap Table")
        {
            this.Width = 51;
            this.Height = 1;
            this.RefreshRateHz = 2;
        }
        public override void SetupPreviewData()
        {
            
        }

        public override void BeforeStart()
        {
            int[] columnWidths = _config.Table.ShowSectors switch
            {
                true => new int[] { 130, 90, 90, 90 },
                false => new int[] { 130 }
            };
            _table = new InfoTable(12, columnWidths);
            this.Width += columnWidths.Sum();
            this.Height += _table.FontHeight * (_config.Table.Rows + 1);
        }

        public override void Render(Graphics g)
        {
            var laps = LapTracker.Instance.Laps.ToList();
            var lapList = laps.OrderByDescending(x => x.Key).Take(_config.Table.Rows);

            switch (_config.Table.ShowSectors)
            {
                case true:
                    {
                        _table?.AddRow("#  ", new string[] { "Time", "S1", "S2", "S3" });

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
                            string sector3 = $"{lap.Value.GetSector3():F3}";

                            TableRow row = new TableRow() { Header = $"{lap.Key}", Columns = new string[] { $"{lapTimeValue}", sector1, sector2, sector3 } };
                            if (!lap.Value.IsValid)
                                row.ColumnColors = new Color[] { Color.OrangeRed, Color.OrangeRed, Color.OrangeRed, Color.OrangeRed };

                            _table?.AddRow(row);
                        }
                        break;
                    }
                case false:
                    {
                        _table?.AddRow("#  ", new string[] { "Time" });

                        foreach (var lap in lapList)
                        {
                            string lapTimeValue = $"--:--.---";
                            if (lap.Value.Time != -1)
                            {
                                TimeSpan best = TimeSpan.FromMilliseconds(lap.Value.Time);
                                lapTimeValue = $"{best:mm\\:ss\\:fff}";
                            }
                            TableRow row = new TableRow() { Header = $"{lap.Key}", Columns = new string[] { $"{lapTimeValue}" } };
                            if (!lap.Value.IsValid)
                                row.ColumnColors = new Color[] { Color.OrangeRed };

                            _table?.AddRow(row);
                        }
                        break;
                    }
            }

            _table?.Draw(g);
        }
    }
}
