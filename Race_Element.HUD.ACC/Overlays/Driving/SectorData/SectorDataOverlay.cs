using Newtonsoft.Json;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.SectorData;

[Overlay(Name = "Sector Data",
Description = "Shows data from previous sectors. New driven sectors appear at the top",
Authors = ["Reinier Klarenberg"])]
internal sealed class SectorDataOverlay(Rectangle rectangle) : ACCOverlay(rectangle, "Sector Data")
{
    private readonly SectorDataConfiguration _config = new();

    /// <summary>
    /// New sector at 0 index;
    /// </summary>
    private readonly List<SectorDataModel> _Sectors = [];
    private SectorDataJob _datajob;

    // Graphics
    private GraphicsGrid _graphicsGrid;
    private Font _font;
    private CachedBitmap[] _columnBackgrounds;

    public sealed override void SetupPreviewData()
    {
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 0, VelocityMin = 101, VelocityMax = 140 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 1, VelocityMin = 56, VelocityMax = 160 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 2, VelocityMin = 88, VelocityMax = 188 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 0, VelocityMin = 101, VelocityMax = 140 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 1, VelocityMin = 56, VelocityMax = 160 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 2, VelocityMin = 88, VelocityMax = 188.37f });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 0, VelocityMin = 101, VelocityMax = 140 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 1, VelocityMin = 56, VelocityMax = 160 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 2, VelocityMin = 88, VelocityMax = 178.31f });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 0, VelocityMin = 101, VelocityMax = 140 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 1, VelocityMin = 56, VelocityMax = 160 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 2, VelocityMin = 88, VelocityMax = 189.33f });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 0, VelocityMin = 101, VelocityMax = 140 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 1, VelocityMin = 56, VelocityMax = 160 });
        _Sectors.Insert(0, new SectorDataModel() { SectorIndex = 2, VelocityMin = 88, VelocityMax = 187.34f });
    }

    public sealed override void BeforeStart()
    {
        float scale = this.Scale;
        if (IsPreviewing) scale = 1f;

        _font = FontUtil.FontSegoeMono(12f * scale);

        int rows = 1; // 1 for header
        rows += _config.Table.Rows;
        int columns = 3;    // sector, vMin, vMax
        _graphicsGrid = new GraphicsGrid(rows, columns);

        float fontHeight = (int)(_font.GetHeight(120));
        int columnHeight = (int)(Math.Ceiling(fontHeight) + 1 * scale);
        int[] columnWidths = [(int)(70f * scale), (int)(75 * scale), (int)(75 * scale)];
        int totalWidth = columnWidths[0] + columnWidths[1] + columnWidths[2];

        // set up backgrounds of each cell
        Color colorDefault = Color.FromArgb(190, Color.Black);
        using HatchBrush columnBrushDefault = new(HatchStyle.LightUpwardDiagonal, colorDefault, Color.FromArgb(colorDefault.A - 25, colorDefault));
        _columnBackgrounds = new CachedBitmap[columns];

        for (int i = 0; i < columns; i++)
        {
            Rectangle rect = new(0, 0, columnWidths[i], columnHeight);
            _columnBackgrounds[i] = new CachedBitmap(columnWidths[i], columnHeight, g =>
            {
                using LinearGradientBrush brush = new(new PointF(columnWidths[i], columnHeight), new PointF(0, 0), Color.FromArgb(90, 0, 0, 0), Color.FromArgb(colorDefault.A, 10, 10, 10));
                g.FillRoundedRectangle(brush, rect, (int)(_config.Table.Roundness * scale));
                g.FillRoundedRectangle(columnBrushDefault, rect, (int)(_config.Table.Roundness * scale));
            });
        }

        DrawableTextCell col1 = new(new RectangleF(0, 0, columnWidths[0], columnHeight), _font);
        col1.CachedBackground = _columnBackgrounds[0];
        col1.UpdateText("Sector");
        DrawableTextCell col2 = new(new RectangleF(col1.Rectangle.X + columnWidths[0], 0, columnWidths[1], columnHeight), _font);
        col2.CachedBackground = _columnBackgrounds[1];
        col2.UpdateText("vMin");
        DrawableTextCell col3 = new(new RectangleF(col2.Rectangle.X + columnWidths[1], 0, columnWidths[2], columnHeight), _font);
        col3.CachedBackground = _columnBackgrounds[2];
        col3.UpdateText("vMax");
        _graphicsGrid.Grid[0][0] = col1;
        _graphicsGrid.Grid[0][1] = col2;
        _graphicsGrid.Grid[0][2] = col3;

        this.Width = totalWidth + 1;
        this.Height = columnHeight * (_config.Table.Rows + 1) + 1; // +1 for header

        RefreshRateHz = 0.5f;
        if (_config.Table.LiveCurrentSector)
            RefreshRateHz = 6f;

        StringFormat sf = new() { Alignment = StringAlignment.Far };
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
                if (column > 0) cell.StringFormat = sf;
                cell.CachedBackground = _columnBackgrounds[column];
                _graphicsGrid.Grid[row][column] = cell;
                cell.UpdateText("");
            }
        }

        if (IsPreviewing)
            return;

        _datajob = new SectorDataJob() { IntervalMillis = 100 };
        _datajob.OnSectorCompleted += SectorCompleted;
        _datajob.Run();

        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;
    }

    private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
    {
        _Sectors.Clear();
        ClearData();
    }

    private void SectorCompleted(object sender, SectorDataModel e)
    {
        _Sectors.Insert(0, e);
        Debug.WriteLine($"Sector {e.SectorIndex + 1} completed:\n{JsonConvert.SerializeObject(e)}");
    }

    public sealed override void BeforeStop()
    {
        _graphicsGrid?.Dispose();
        _font?.Dispose();
        for (int i = 0; i < _columnBackgrounds.Length; i++)
            _columnBackgrounds[i].Dispose();

        if (IsPreviewing) return;

        _datajob.OnSectorCompleted -= SectorCompleted;
        _datajob.CancelJoin();
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
    }

    public sealed override void Render(Graphics g)
    {
        bool hasSetLiveSector = false;
        if (!IsPreviewing && _config.Table.LiveCurrentSector && _datajob._currentData != null)
        {
            DrawableTextCell sectorCell = (DrawableTextCell)_graphicsGrid.Grid[1][0];
            sectorCell?.UpdateText($"{_datajob._currentData.SectorIndex + 1}..");

            DrawableTextCell vMinCell = (DrawableTextCell)_graphicsGrid.Grid[1][1];
            vMinCell?.UpdateText($"{_datajob._currentData.VelocityMin:F2}");

            DrawableTextCell vMaxCell = (DrawableTextCell)_graphicsGrid.Grid[1][2];
            vMaxCell?.UpdateText($"{_datajob._currentData.VelocityMax:F2}");
            hasSetLiveSector = true;
        }
        for (int i = 0; i < _config.Table.Rows; i++)
        {
            if (i >= _Sectors.Count)
                break;

            if (hasSetLiveSector && i >= _config.Table.Rows - 1)
                break;

            int baseGridRow = (hasSetLiveSector ? 2 : 1);
            if (baseGridRow >= _graphicsGrid.Columns)
                break;

            DrawableTextCell sectorCell = (DrawableTextCell)_graphicsGrid.Grid[baseGridRow + i][0];
            sectorCell?.UpdateText($"{_Sectors[i].SectorIndex + 1}");

            DrawableTextCell vMinCell = (DrawableTextCell)_graphicsGrid.Grid[baseGridRow + i][1];
            vMinCell?.UpdateText($"{_Sectors[i].VelocityMin:F2}");

            DrawableTextCell vMaxCell = (DrawableTextCell)_graphicsGrid.Grid[baseGridRow + i][2];
            vMaxCell?.UpdateText($"{_Sectors[i].VelocityMax:F2}");
        }

        _graphicsGrid?.Draw(g);
    }

    private void ClearData()
    {
        for (int i = 0; i < _config.Table.Rows; i++)
        {
            DrawableTextCell sectorCell = (DrawableTextCell)_graphicsGrid.Grid[1 + i][0];
            sectorCell?.UpdateText($"");

            DrawableTextCell vMinCell = (DrawableTextCell)_graphicsGrid.Grid[1 + i][1];
            vMinCell?.UpdateText($"");

            DrawableTextCell vMaxCell = (DrawableTextCell)_graphicsGrid.Grid[1 + i][2];
            vMaxCell?.UpdateText($"");
        }
    }
}
