using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
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
        private CachedBitmap[] _columnBackgrounds;

        public LapTableOverlay(Rectangle rectangle) : base(rectangle, "Lap Table") => this.RefreshRateHz = 30;

        public override void BeforeStart()
        {
            _font = FontUtil.FontSegoeMono(12 * Scale);

            int rows = 1;
            rows += _config.Table.Rows;
            int columns = 2;
            if (_config.Table.ShowSectors) columns += 3;

            _graphicsGrid = new GraphicsGrid(rows, columns);

            float fontHeight = (int)(_font.GetHeight(120));
            int columnHeight = (int)(fontHeight - 2f * Scale);
            int[] columnWidths = new int[] { (int)(45f * Scale), (int)(130f * Scale), (int)(90f * Scale), (int)(90f * Scale), (int)(90f * Scale) };
            int totalWidth = columnWidths[0] + columnWidths[1];

            // set up background rendering
            Color color = Color.FromArgb(230, Color.Black);
            using HatchBrush hatchBrush = new HatchBrush(HatchStyle.LightUpwardDiagonal, color, Color.FromArgb(color.A - 75, color));
            _columnBackgrounds = new CachedBitmap[columns];
            for (int i = 0; i < columns; i++)
                _columnBackgrounds[i] = new CachedBitmap(columnWidths[i], columnHeight, g => g.FillRoundedRectangle(hatchBrush, new Rectangle(0, 0, columnWidths[i], columnHeight), (int)(3 * Scale)));


            // add header row, base columns
            DrawableTextCell col0 = new DrawableTextCell(new Rectangle(0, 0, columnWidths[0], columnHeight), _font);
            col0.CachedBackground = _columnBackgrounds[0];
            col0.UpdateText("#");
            _graphicsGrid.Grid[0][0] = col0;

            DrawableTextCell col1 = new DrawableTextCell(new RectangleF(col0.Rectangle.Width, 0, columnWidths[1], columnHeight), _font);
            col1.CachedBackground = _columnBackgrounds[1];
            col1.UpdateText("Time");
            _graphicsGrid.Grid[0][1] = col1;

            // add header columns for sectors
            if (_config.Table.ShowSectors)
            {
                totalWidth += columnWidths[2] + columnWidths[3] + columnWidths[4];

                DrawableTextCell col2 = new DrawableTextCell(new RectangleF(col1.Rectangle.X + columnWidths[1], 0, columnWidths[2], columnHeight), _font);
                col2.CachedBackground = _columnBackgrounds[2];
                col2.UpdateText("S1");
                DrawableTextCell col3 = new DrawableTextCell(new RectangleF(col2.Rectangle.X + columnWidths[2], 0, columnWidths[3], columnHeight), _font);
                col3.CachedBackground = _columnBackgrounds[3];
                col3.UpdateText("S2");
                DrawableTextCell col4 = new DrawableTextCell(new RectangleF(col3.Rectangle.X + columnWidths[3], 0, columnWidths[4], columnHeight), _font);
                col4.CachedBackground = _columnBackgrounds[4];
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
                    cell.CachedBackground = _columnBackgrounds[column];
                    _graphicsGrid.Grid[row][column] = cell;
                }
            }
        }

        public override void BeforeStop()
        {
            _graphicsGrid?.Dispose();
            _font?.Dispose();
            for (int i = 0; i < _columnBackgrounds.Length; i++)
                _columnBackgrounds[i].Dispose();
        }

        public override void Render(Graphics g)
        {
            var laps = LapTracker.Instance.Laps.ToList();
            var lapList = laps.OrderByDescending(x => x.Key).Take(_config.Table.Rows);

            int row = 1;
            foreach (var lap in lapList)
            {
                Brush textBrush = lap.Value.IsValid ? Brushes.White : Brushes.OrangeRed;

                ((DrawableTextCell)_graphicsGrid.Grid[row][0]).TextBrush = textBrush;
                DrawableTextCell lapCell = (DrawableTextCell)_graphicsGrid.Grid[row][0];
                lapCell.TextBrush = textBrush;
                lapCell.UpdateText($"{lap.Key}");

                string lapTimeValue = $"--:--.---";
                if (lap.Value.Time != -1)
                {
                    TimeSpan best = TimeSpan.FromMilliseconds(lap.Value.Time);
                    lapTimeValue = $"{best:mm\\:ss\\:fff}";
                }

                ((DrawableTextCell)_graphicsGrid.Grid[row][1]).TextBrush = textBrush;
                ((DrawableTextCell)_graphicsGrid.Grid[row][1]).UpdateText($"{lapTimeValue}");

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
