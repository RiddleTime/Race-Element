using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.OverlayLapTimeTable
{
    [Overlay(Name = "Lap Table", Description = "A table showing time for each lap and optionally sectors.", OverlayType = OverlayType.Drive, Version = 1.00,
        OverlayCategory = OverlayCategory.Lap)]
    internal sealed class LapTableOverlay : AbstractOverlay
    {
        private readonly LapTimeTableConfiguration _config = new();
        private GraphicsGrid _graphicsGrid;
        private Font _font;
        private CachedBitmap[] _columnBackgroundsValid;
        private CachedBitmap[] _columnBackgroundsRed;
        private CachedBitmap[] _columnBackgroundsGreen;
        private CachedBitmap[] _columnBackgroundsPurple;

        private List<KeyValuePair<int, DbLapData>> _storedLaps;
        private bool _dataIsPreview = false;

        public LapTableOverlay(Rectangle rectangle) : base(rectangle, "Lap Table")
        {
            this.RefreshRateHz = 2;
            _storedLaps = new List<KeyValuePair<int, DbLapData>>();
        }

        public override void SetupPreviewData()
        {
            _dataIsPreview = true;

            Dictionary<int, DbLapData> Laps = new();
            Random rand = new();
            int maxSectorDeviation = 10000;
            int s1 = 28525 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
            int s2 = 38842 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
            int s3 = 36840 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
            int startLapIndex = 100 + rand.Next(-50, 800);

            for (int i = startLapIndex; i < _config.Table.Rows + startLapIndex; i++)
            {
                DbLapData randomData = new()
                {
                    Index = i,
                    LapType = Broadcast.LapType.Regular,
                    IsValid = rand.Next(0, 10) > 3,
                    Sector1 = s1 + rand.Next(-200, 200),
                    Sector2 = s2 + rand.Next(-300, 300),
                    Sector3 = s3 + rand.Next(-300, 500),
                };
                randomData.Time = randomData.Sector1 + randomData.Sector2 + randomData.Sector3;
                Laps.Add(i, randomData);
            }

            _storedLaps = Laps.OrderByDescending(x => x.Key).ToList();
        }

        public override void BeforeStart()
        {
            float scale = this.Scale;
            if (_dataIsPreview) scale = 1f;

            _font = FontUtil.FontSegoeMono(12f * scale);

            int rows = 1;
            rows += _config.Table.Rows;
            int columns = 2;
            if (_config.Table.ShowSectors) columns += 3;

            _graphicsGrid = new GraphicsGrid(rows, columns);

            float fontHeight = (int)(_font.GetHeight(120));
            int columnHeight = (int)(Math.Ceiling(fontHeight) + 1 * scale);
            int[] columnWidths = new int[] { (int)(45f * scale), (int)(100f * scale), (int)(78f * scale), (int)(78f * scale), (int)(78f * scale) };
            int totalWidth = columnWidths[0] + columnWidths[1];

            // set up backgrounds and invalid ones
            Color colorDefault = Color.FromArgb(190, Color.Black);
            Color colorRed = Color.FromArgb(150, Color.Red);
            Color colorGreen = Color.FromArgb(150, Color.LimeGreen);
            Color colorPurple = Color.FromArgb(150, Color.MediumPurple);
            using HatchBrush columnBrushDefault = new(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorDefault.A - 25, colorDefault));
            using HatchBrush columnBrushRed = new(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorRed.A - 75, colorRed));
            using HatchBrush columnBrushGreen = new(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorGreen.A - 25, colorGreen));
            using HatchBrush columnBrushPurple = new(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorPurple.A - 25, colorPurple));
            _columnBackgroundsValid = new CachedBitmap[columns];
            _columnBackgroundsRed = new CachedBitmap[columns];
            _columnBackgroundsGreen = new CachedBitmap[columns];
            _columnBackgroundsPurple = new CachedBitmap[columns];
            for (int i = 0; i < columns; i++)
            {
                Rectangle rect = new(0, 0, columnWidths[i], columnHeight);
                _columnBackgroundsValid[i] = new CachedBitmap(columnWidths[i], columnHeight, g =>
                {
                    using LinearGradientBrush brush = new(new PointF(columnWidths[i], columnHeight), new PointF(0, 0), Color.FromArgb(90, 0, 0, 0), Color.FromArgb(colorDefault.A, 10, 10, 10));
                    g.FillRoundedRectangle(brush, rect, (int)(_config.Table.Roundness * scale));
                    g.FillRoundedRectangle(columnBrushDefault, rect, (int)(_config.Table.Roundness * scale));
                });
                _columnBackgroundsRed[i] = new CachedBitmap(columnWidths[i], columnHeight, g =>
                {
                    using LinearGradientBrush brush = new(new PointF(0, 0), new PointF(columnWidths[i], columnHeight), Color.FromArgb(90, 0, 0, 0), Color.FromArgb(colorRed.A, colorRed.R, 10, 10));
                    g.FillRoundedRectangle(brush, rect, (int)(_config.Table.Roundness * scale));
                    g.FillRoundedRectangle(columnBrushRed, rect, (int)(_config.Table.Roundness * scale));
                });
                _columnBackgroundsGreen[i] = new CachedBitmap(columnWidths[i], columnHeight, g =>
                {
                    using LinearGradientBrush brush = new(new PointF(0, 0), new PointF(columnWidths[i], columnHeight), Color.FromArgb(90, 0, 0, 0), Color.FromArgb(colorGreen.A, colorGreen.R, 10, 10));
                    g.FillRoundedRectangle(brush, rect, (int)(_config.Table.Roundness * scale));
                    g.FillRoundedRectangle(columnBrushGreen, rect, (int)(_config.Table.Roundness * scale));
                });
                _columnBackgroundsPurple[i] = new CachedBitmap(columnWidths[i], columnHeight, g =>
                {
                    using LinearGradientBrush brush = new(new PointF(0, 0), new PointF(columnWidths[i], columnHeight), Color.FromArgb(90, 0, 0, 0), Color.FromArgb(colorPurple.A, colorPurple.R, 10, 10));
                    g.FillRoundedRectangle(brush, rect, (int)(_config.Table.Roundness * scale));
                    g.FillRoundedRectangle(columnBrushPurple, rect, (int)(_config.Table.Roundness * scale));
                });
            }

            // add header row, base columns
            DrawableTextCell col0 = new(new Rectangle(0, 0, columnWidths[0], columnHeight), _font);
            _graphicsGrid.Grid[0][0] = col0;

            DrawableTextCell col1 = new(new RectangleF(col0.Rectangle.Width, 0, columnWidths[1], columnHeight), _font);
            col1.CachedBackground = _columnBackgroundsValid[1];
            col1.UpdateText("Time");
            _graphicsGrid.Grid[0][1] = col1;

            // add header columns for sectors
            if (_config.Table.ShowSectors)
            {
                totalWidth += columnWidths[2] + columnWidths[3] + columnWidths[4];

                DrawableTextCell col2 = new(new RectangleF(col1.Rectangle.X + columnWidths[1], 0, columnWidths[2], columnHeight), _font);
                col2.CachedBackground = _columnBackgroundsValid[2];
                col2.UpdateText("S1");
                DrawableTextCell col3 = new(new RectangleF(col2.Rectangle.X + columnWidths[2], 0, columnWidths[3], columnHeight), _font);
                col3.CachedBackground = _columnBackgroundsValid[3];
                col3.UpdateText("S2");
                DrawableTextCell col4 = new(new RectangleF(col3.Rectangle.X + columnWidths[3], 0, columnWidths[4], columnHeight), _font);
                col4.CachedBackground = _columnBackgroundsValid[4];
                col4.UpdateText("S3");
                _graphicsGrid.Grid[0][2] = col2;
                _graphicsGrid.Grid[0][3] = col3;
                _graphicsGrid.Grid[0][4] = col4;
            }

            this.Width = totalWidth + 1;
            this.Height = columnHeight * (_config.Table.Rows + 1) + 1; // +1 for header

            // config data rows
            for (int row = 1; row <= _config.Table.Rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int x = columnWidths.Take(column).Sum();
                    int y = row * columnHeight;
                    int width = columnWidths[column];
                    RectangleF rect = new(x, y, width, columnHeight);
                    DrawableTextCell cell = new(rect, _font);
                    cell.CachedBackground = _columnBackgroundsValid[column];
                    _graphicsGrid.Grid[row][column] = cell;
                }
            }

            RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStarted;
        }

        private void OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
        {
            ClearData();
        }

        private void ClearData()
        {
            for (int row = 1; row < this._graphicsGrid.Rows; row++)
                for (int column = 0; column < this._graphicsGrid.Columns; column++)
                {
                    var cell = (DrawableTextCell)_graphicsGrid.Grid[row][column];
                    cell.CachedBackground = this._columnBackgroundsValid[column];
                    cell.UpdateText("");
                }
        }

        public override void BeforeStop()
        {
            RaceSessionTracker.Instance.OnNewSessionStarted -= OnNewSessionStarted;

            _graphicsGrid?.Dispose();
            _font?.Dispose();
            for (int i = 0; i < _columnBackgroundsValid.Length; i++)
                _columnBackgroundsValid[i].Dispose();
        }

        public override void Render(Graphics g)
        {
            if (!_dataIsPreview)
                _storedLaps = LapTracker.Instance.Laps.OrderByDescending(x => x.Key).Take(_config.Table.Rows).ToList();
            //if (_storedLaps.Count == 0) return;

            int bestLapInLobby = -1;
            if (broadCastRealTime.BestSessionLap != null && broadCastRealTime.BestSessionLap.LaptimeMS.HasValue)
                bestLapInLobby = broadCastRealTime.BestSessionLap.LaptimeMS.Value;


            int fastestLapIndex = LapTracker.Instance.Laps.GetFastestLapIndex();
            DbLapData bestLap = null;
            if (fastestLapIndex != -1)
                bestLap = LapTracker.Instance.Laps.FirstOrDefault(x => x.Value.Index == fastestLapIndex).Value;

            int fastestSector1 = LapTracker.Instance.Laps.GetFastestSector(1);
            int fastestSector2 = LapTracker.Instance.Laps.GetFastestSector(2);
            int fastestSector3 = LapTracker.Instance.Laps.GetFastestSector(3);

            if (_dataIsPreview)
            {
                Random rand = new();
                int maxSectorDeviation = 10000;
                fastestSector1 = 23525 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
                fastestSector2 = 37842 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
                fastestSector3 = 35840 + rand.Next(-maxSectorDeviation, maxSectorDeviation);
                bestLap = new DbLapData { Sector1 = fastestSector1 - 250, Sector2 = fastestSector2 + 50, Sector3 = fastestSector3 + 30 };
            }

            int row = 1;
            foreach (var lap in _storedLaps)
            {
                DrawableTextCell lapCell = (DrawableTextCell)_graphicsGrid.Grid[row][0];
                lapCell.UpdateText($"{lap.Key}");

                string lapTimeValue = $"--:--.---";
                if (lap.Value.Time != -1)
                {
                    TimeSpan best = TimeSpan.FromMilliseconds(lap.Value.Time);
                    lapTimeValue = $"{best:mm\\:ss\\.fff}";
                }

                DrawableTextCell lapTimeCell = (DrawableTextCell)_graphicsGrid.Grid[row][1];
                lapTimeCell.CachedBackground = _columnBackgroundsValid[1];
                if (bestLap != null && lap.Value.Time == bestLap.Sector1 + bestLap.Sector2 + bestLap.Sector3)
                    lapTimeCell.CachedBackground = _columnBackgroundsGreen[1];

                if (lap.Value.IsValid && lap.Value.Time == bestLapInLobby)
                    lapTimeCell.CachedBackground = _columnBackgroundsPurple[1];

                if (!lap.Value.IsValid)
                    lapTimeCell.CachedBackground = _columnBackgroundsRed[1];
                lapTimeCell.UpdateText($"{lapTimeValue}");

                if (_config.Table.ShowSectors)
                {
                    UpdateSectorTime(row, 2, 1, lap, lap.Value.Sector1, fastestSector1, bestLap == null ? int.MaxValue : bestLap.Sector1);
                    UpdateSectorTime(row, 3, 2, lap, lap.Value.Sector2, fastestSector2, bestLap == null ? int.MaxValue : bestLap.Sector2);
                    UpdateSectorTime(row, 4, 3, lap, lap.Value.Sector3, fastestSector3, bestLap == null ? int.MaxValue : bestLap.Sector3);
                }

                row++;
            }

            _graphicsGrid?.Draw(g);
        }

        /// <summary>
        /// All times are millisecond based integers and need to be divided by 1000d
        /// </summary>
        /// <param name="row">the row in the graphics grid</param>
        /// <param name="column">the column in the graphics grid</param>
        /// <param name="sector">the sector (1,2,3...)</param>
        /// <param name="lap">the actual lap</param>
        /// <param name="sectorTime">sector time</param>
        /// <param name="fastestSectorTime">sector time based on fastest sector time disregarding whether it's the best valid lap</param>
        /// <param name="bestSectorTime">sector time based on best valid lap</param>
        private void UpdateSectorTime(int row, int column, int sector, KeyValuePair<int, DbLapData> lap, int sectorTime, int fastestSectorTime, int bestSectorTime)
        {
            DrawableTextCell sectorCell = (DrawableTextCell)_graphicsGrid.Grid[row][column];
            sectorCell.UpdateText(FormatSectorTime(sectorTime));

            sectorCell.CachedBackground = _columnBackgroundsValid[column];

            if (!lap.Value.IsValid)
            {
                if (sector == lap.Value.InvalidatedSectorIndex + 1)
                    sectorCell.CachedBackground = _columnBackgroundsRed[column];
            }
            else
            {
                if (sectorTime == bestSectorTime)
                    sectorCell.CachedBackground = _columnBackgroundsGreen[column];
                else if (sectorTime == fastestSectorTime)
                    sectorCell.CachedBackground = _columnBackgroundsPurple[column];
            }
        }

        /// <summary>
        /// Creates a string reprenting the given sector time.
        ///  If the given sector time is equal or higher than 1,000,000 milliseconds, the returned value will be '-' to prevent overflow.
        /// </summary>
        private string FormatSectorTime(int sectorTimeMilliseconds) => sectorTimeMilliseconds > 999_999 ? "-" : $"{sectorTimeMilliseconds / 1000d:F3}";

    }
}
