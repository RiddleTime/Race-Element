using RaceElement.Data.ACC.Database.LapDataDB;
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
    [Overlay(Name = "Lap Table", Description = "A table showing time for each lap and optionally sectors.", OverlayType = OverlayType.Release, Version = 1.00,
        OverlayCategory = OverlayCategory.Lap)]
    internal sealed class LapTableOverlay : AbstractOverlay
    {
        private readonly LapTimeTableConfiguration _config = new LapTimeTableConfiguration();
        private GraphicsGrid _graphicsGrid;
        private Font _font;
        private CachedBitmap[] _columnBackgroundsValid;
        private CachedBitmap[] _columnBackgroundsInvalid;

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

            Dictionary<int, DbLapData> Laps = new Dictionary<int, DbLapData>();
            Random rand = new Random();
            int min = -10000, max = 10000;
            int s1 = 28525 + rand.Next(min, max);
            int s2 = 38842 + rand.Next(min, max);
            int s3 = 36840 + rand.Next(min, max);
            int startLapIndex = 100 + rand.Next(-50, 800);
            for (int i = startLapIndex; i < _config.Table.Rows + startLapIndex; i++)
            {
                DbLapData randomData = new DbLapData()
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

            _font = FontUtil.FontSegoeMono(12 * scale);

            int rows = 1;
            rows += _config.Table.Rows;
            int columns = 2;
            if (_config.Table.ShowSectors) columns += 3;

            _graphicsGrid = new GraphicsGrid(rows, columns);

            float fontHeight = (int)(_font.GetHeight(120));
            int columnHeight = (int)(Math.Ceiling(fontHeight) + 1 * scale);
            int[] columnWidths = new int[] { (int)(45f * scale), (int)(100f * scale), (int)(75f * scale), (int)(75f * scale), (int)(75f * scale) };
            int totalWidth = columnWidths[0] + columnWidths[1];

            // set up backgrounds and invalid ones
            Color colorValid = Color.FromArgb(230, Color.Black);
            Color colorInvalid = Color.FromArgb(170, Color.Red);
            using HatchBrush columnBrushValid = new HatchBrush(HatchStyle.LightUpwardDiagonal, colorValid, Color.FromArgb(colorValid.A - 75, colorValid));
            using HatchBrush columnBrushInvalid = new HatchBrush(HatchStyle.LightUpwardDiagonal, colorInvalid, Color.FromArgb(colorInvalid.A - 125, colorInvalid));
            _columnBackgroundsValid = new CachedBitmap[columns];
            _columnBackgroundsInvalid = new CachedBitmap[columns];
            for (int i = 0; i < columns; i++)
                _columnBackgroundsValid[i] = new CachedBitmap(columnWidths[i], columnHeight, g => g.FillRoundedRectangle(columnBrushValid, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale)));
            for (int i = 0; i < columns; i++)
                _columnBackgroundsInvalid[i] = new CachedBitmap(columnWidths[i], columnHeight, g => g.FillRoundedRectangle(columnBrushInvalid, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(_config.Table.Roundness * scale)));

            // add header row, base columns
            DrawableTextCell col0 = new DrawableTextCell(new Rectangle(0, 0, columnWidths[0], columnHeight), _font);
            _graphicsGrid.Grid[0][0] = col0;

            DrawableTextCell col1 = new DrawableTextCell(new RectangleF(col0.Rectangle.Width, 0, columnWidths[1], columnHeight), _font);
            col1.CachedBackground = _columnBackgroundsValid[1];
            col1.UpdateText("Time");
            _graphicsGrid.Grid[0][1] = col1;

            // add header columns for sectors
            if (_config.Table.ShowSectors)
            {
                totalWidth += columnWidths[2] + columnWidths[3] + columnWidths[4];

                DrawableTextCell col2 = new DrawableTextCell(new RectangleF(col1.Rectangle.X + columnWidths[1], 0, columnWidths[2], columnHeight), _font);
                col2.CachedBackground = _columnBackgroundsValid[2];
                col2.UpdateText("S1");
                DrawableTextCell col3 = new DrawableTextCell(new RectangleF(col2.Rectangle.X + columnWidths[2], 0, columnWidths[3], columnHeight), _font);
                col3.CachedBackground = _columnBackgroundsValid[3];
                col3.UpdateText("S2");
                DrawableTextCell col4 = new DrawableTextCell(new RectangleF(col3.Rectangle.X + columnWidths[3], 0, columnWidths[4], columnHeight), _font);
                col4.CachedBackground = _columnBackgroundsValid[4];
                col4.UpdateText("S3");
                _graphicsGrid.Grid[0][2] = col2;
                _graphicsGrid.Grid[0][3] = col3;
                _graphicsGrid.Grid[0][4] = col4;
            }

            this.Width = totalWidth;
            this.Height = columnHeight * (_config.Table.Rows + 1); // +1 for header

            // config data rows
            for (int row = 1; row <= _config.Table.Rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int x = columnWidths.Take(column).Sum();
                    int y = row * columnHeight;
                    int width = columnWidths[column];
                    RectangleF rect = new RectangleF(x, y, width, columnHeight);
                    DrawableTextCell cell = new DrawableTextCell(rect, _font);
                    cell.CachedBackground = _columnBackgroundsValid[column];
                    _graphicsGrid.Grid[row][column] = cell;
                }
            }
        }

        public override void BeforeStop()
        {
            _graphicsGrid?.Dispose();
            _font?.Dispose();
            for (int i = 0; i < _columnBackgroundsValid.Length; i++)
                _columnBackgroundsValid[i].Dispose();
        }

        public override void Render(Graphics g)
        {
            if (!_dataIsPreview)
                _storedLaps = LapTracker.Instance.Laps.OrderByDescending(x => x.Key).Take(_config.Table.Rows).ToList();

            int row = 1;
            foreach (var lap in _storedLaps)
            {
                DrawableTextCell lapCell = (DrawableTextCell)_graphicsGrid.Grid[row][0];
                lapCell.UpdateText($"{lap.Key}");

                string lapTimeValue = $"--:--.---";
                if (lap.Value.Time != -1)
                {
                    TimeSpan best = TimeSpan.FromMilliseconds(lap.Value.Time);
                    lapTimeValue = $"{best:mm\\:ss\\:fff}";
                }

                DrawableTextCell lapTimeCell = (DrawableTextCell)_graphicsGrid.Grid[row][1];
                lapTimeCell.CachedBackground = lap.Value.IsValid ? _columnBackgroundsValid[1] : _columnBackgroundsInvalid[1];
                lapTimeCell.UpdateText($"{lapTimeValue}");

                if (_config.Table.ShowSectors)
                {
                    ((DrawableTextCell)_graphicsGrid.Grid[row][2]).UpdateText($"{lap.Value.GetSector1():F3}");
                    ((DrawableTextCell)_graphicsGrid.Grid[row][3]).UpdateText($"{lap.Value.GetSector2():F3}");
                    ((DrawableTextCell)_graphicsGrid.Grid[row][4]).UpdateText($"{lap.Value.GetSector3():F3}");
                }

                row++;
            }

            _graphicsGrid?.Draw(g);
        }
    }
}
