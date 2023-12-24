using RaceElement.Data.ACC.Cars;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.OverlayCarInfo
{
    [Overlay(Name = "Car Info", Version = 1.00, OverlayType = OverlayType.Drive,
        Description = "A panel showing the damage time. Optionally showing current tyre set, fuel per lap, exhaust temp and water temp.",
        OverlayCategory = OverlayCategory.Car)]
    internal sealed class CarInfoOverlay : AbstractOverlay
    {
        private readonly CarInfoConfiguration _config = new();
        private sealed class CarInfoConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Info Panel", "Show or hide additional information in the panel.")]
            public InfoPanelGrouping InfoPanel { get; set; } = new InfoPanelGrouping();
            public class InfoPanelGrouping
            {
                [ToolTip("Displays your current tyre set")]
                public bool TyreSet { get; set; } = true;

                [ToolTip("Displays the average fuel usage over the past 3 laps.\n(uses last lap info if not enough data)")]
                public bool FuelPerLap { get; set; } = false;

                [ToolTip("Displays the exhaust temperature.")]
                public bool ExhaustTemp { get; set; } = false;

                [ToolTip("Displays the water temperature of the engine.")]
                public bool WaterTemp { get; set; } = false;
            }

            public CarInfoConfiguration() => this.AllowRescale = true;
        }

        private Font _font;
        private DrawableTextCell _damageValue;
        private DrawableTextCell _tyreSetValue;
        private DrawableTextCell _fuelPerLapValue;
        private DrawableTextCell _exhaustValue;
        private DrawableTextCell _waterValue;

        private GraphicsGrid _graphicsGrid;

        public CarInfoOverlay(Rectangle rectangle) : base(rectangle, "Car Info")
        {
            this.RefreshRateHz = 1;
        }

        public sealed override void BeforeStart()
        {
            _font = FontUtil.FontSegoeMono(10f * this.Scale);

            int rows = 1;

            if (_config.InfoPanel.TyreSet) rows++;
            if (_config.InfoPanel.FuelPerLap) rows++;
            if (_config.InfoPanel.ExhaustTemp) rows++;
            if (_config.InfoPanel.WaterTemp) rows++;
            _graphicsGrid = new GraphicsGrid(rows, 2);

            float fontHeight = (int)(_font.GetHeight(120));
            int columnHeight = (int)(fontHeight - 2f * Scale);
            int headerColumnWidth = (int)Math.Ceiling(76f * Scale);
            int valueColumnWidth = (int)Math.Ceiling(56f * Scale);
            float roundingRadius = 6f * Scale;
            RectangleF headerRectangle = new(0, 0, headerColumnWidth, columnHeight);
            RectangleF valueRectangle = new(headerColumnWidth, 0, valueColumnWidth, columnHeight);

            // create value and header backgrounds
            Color accentColor = Color.FromArgb(25, 255, 0, 0);
            CachedBitmap headerBackground = new(headerColumnWidth, columnHeight, g =>
            {
                Rectangle panelRect = new(0, 0, headerColumnWidth, columnHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, 0, 0, (int)roundingRadius);
                g.FillPath(new SolidBrush(Color.FromArgb(225, 10, 10, 10)), path);
                g.DrawLine(new Pen(accentColor), 0 + roundingRadius / 2, columnHeight, headerColumnWidth, columnHeight - 1 * Scale);
            });
            CachedBitmap valueBackground = new(valueColumnWidth, columnHeight, g =>
            {
                Rectangle panelRect = new(0, 0, valueColumnWidth, columnHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, (int)roundingRadius, 0, 0);
                g.FillPath(new SolidBrush(Color.FromArgb(225, 0, 0, 0)), path);
                g.DrawLine(new Pen(accentColor), 0, columnHeight - 1 * Scale, valueColumnWidth, columnHeight - 1 * Scale);
            });

            // add data rows

            // Damage
            int currentRow = 0;
            DrawableTextCell damageHeader = new(headerRectangle, _font);
            damageHeader.CachedBackground = headerBackground;
            damageHeader.StringFormat.Alignment = StringAlignment.Near;
            damageHeader.UpdateText("Damage");
            _graphicsGrid.Grid[currentRow][0] = damageHeader;
            _damageValue = new DrawableTextCell(valueRectangle, _font);
            _damageValue.CachedBackground = valueBackground;
            _damageValue.StringFormat.Alignment = StringAlignment.Far;
            _graphicsGrid.Grid[currentRow][1] = _damageValue;

            if (_config.InfoPanel.TyreSet)
            {
                currentRow++;
                headerRectangle.Offset(0, columnHeight);
                valueRectangle.Offset(0, columnHeight);

                DrawableTextCell header = new(headerRectangle, _font);
                header.CachedBackground = headerBackground;
                header.StringFormat.Alignment = StringAlignment.Near;
                header.UpdateText("Tyre set");
                _graphicsGrid.Grid[currentRow][0] = header;

                _tyreSetValue = new DrawableTextCell(valueRectangle, _font);
                _tyreSetValue.CachedBackground = valueBackground;
                _tyreSetValue.StringFormat.Alignment = StringAlignment.Far;
                _graphicsGrid.Grid[currentRow][1] = _tyreSetValue;
            }

            if (_config.InfoPanel.FuelPerLap)
            {
                currentRow++;
                headerRectangle.Offset(0, columnHeight);
                valueRectangle.Offset(0, columnHeight);

                DrawableTextCell header = new(headerRectangle, _font);
                header.CachedBackground = headerBackground;
                header.StringFormat.Alignment = StringAlignment.Near;
                header.UpdateText("Fuel/Lap");
                _graphicsGrid.Grid[currentRow][0] = header;

                _fuelPerLapValue = new DrawableTextCell(valueRectangle, _font);
                _fuelPerLapValue.CachedBackground = valueBackground;
                _fuelPerLapValue.StringFormat.Alignment = StringAlignment.Far;
                _graphicsGrid.Grid[currentRow][1] = _fuelPerLapValue;
            }

            if (_config.InfoPanel.ExhaustTemp)
            {
                currentRow++;
                headerRectangle.Offset(0, columnHeight);
                valueRectangle.Offset(0, columnHeight);

                DrawableTextCell header = new(headerRectangle, _font);
                header.CachedBackground = headerBackground;
                header.StringFormat.Alignment = StringAlignment.Near;
                header.UpdateText("Exhaust");
                _graphicsGrid.Grid[currentRow][0] = header;

                _exhaustValue = new DrawableTextCell(valueRectangle, _font);
                _exhaustValue.CachedBackground = valueBackground;
                _exhaustValue.StringFormat.Alignment = StringAlignment.Far;
                _graphicsGrid.Grid[currentRow][1] = _exhaustValue;
            }

            if (_config.InfoPanel.WaterTemp)
            {
                currentRow++;
                headerRectangle.Offset(0, columnHeight);
                valueRectangle.Offset(0, columnHeight);

                DrawableTextCell header = new(headerRectangle, _font);
                header.CachedBackground = headerBackground;
                header.StringFormat.Alignment = StringAlignment.Near;
                header.UpdateText("Water");
                _graphicsGrid.Grid[currentRow][0] = header;

                _waterValue = new DrawableTextCell(valueRectangle, _font);
                _waterValue.CachedBackground = valueBackground;
                _waterValue.StringFormat.Alignment = StringAlignment.Far;
                _graphicsGrid.Grid[currentRow][1] = _waterValue;
            }

            // set HUD Width + Height based on amount of rows and columns
            Width = headerColumnWidth + valueColumnWidth;
            Height = rows * columnHeight;
        }

        public override void BeforeStop()
        {
            _font?.Dispose();
            _graphicsGrid?.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            float totalRepairTime = Damage.GetTotalRepairTime(pagePhysics);
            _damageValue.TextBrush = totalRepairTime > 0 ? Brushes.OrangeRed : Brushes.White;
            _damageValue.UpdateText($"{totalRepairTime:F1}");

            if (_config.InfoPanel.TyreSet)
                _tyreSetValue.UpdateText($"{pageGraphics.currentTyreSet}");

            if (_config.InfoPanel.FuelPerLap)
            {
                float fuelXLap = LapTracker.Instance.Laps.GetAverageFuelUsage(3);
                if (fuelXLap != -1)
                    fuelXLap /= 1000f;
                else fuelXLap = pageGraphics.FuelXLap;
                _fuelPerLapValue.UpdateText($"{fuelXLap:F3}");
            }

            if (_config.InfoPanel.ExhaustTemp)
                _exhaustValue.UpdateText($"{pageGraphics.ExhaustTemperature:F0} °C");

            if (_config.InfoPanel.WaterTemp)
                _waterValue.UpdateText($"{pagePhysics.WaterTemp:F0} °C");

            _graphicsGrid?.Draw(g);
        }
    }
}
