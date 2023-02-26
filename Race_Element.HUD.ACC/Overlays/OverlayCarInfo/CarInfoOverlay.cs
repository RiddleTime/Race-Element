using RaceElement.Data.ACC.Cars;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.InfoPanel;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.OverlayCarInfo
{
    [Overlay(Name = "Car Info", Version = 1.00, OverlayType = OverlayType.Release,
        Description = "A panel showing the damage time. Optionally showing current tyre set, fuel per lap, exhaust temp and water temp.",
        OverlayCategory = OverlayCategory.Car)]
    internal sealed class CarInfoOverlay : AbstractOverlay
    {
        private readonly CarInfoConfiguration _config = new CarInfoConfiguration();
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
        private PanelText _damageHeader;
        private PanelText _damageValue;
        private PanelText _fuelPerLapHeader;
        private PanelText _fuelPerLapValue;
        private PanelText _tyreSetHeader;
        private PanelText _tyreSetValue;
        private PanelText _exhaustHeader;
        private PanelText _exhaustValue;
        private PanelText _waterHeader;
        private PanelText _waterValue;

        public CarInfoOverlay(Rectangle rectangle) : base(rectangle, "Car Info")
        {
            this.RefreshRateHz = 1;
        }

        public sealed override void BeforeStart()
        {
            _font = FontUtil.FontSegoeMono(10f * this.Scale);
            int lineHeight = _font.Height + 1;
            int unscaledHeaderWidth = 80;
            int unscaledValueWidth = 75;

            int headerWidth = (int)(unscaledHeaderWidth * this.Scale);
            int valueWidth = (int)(unscaledValueWidth * this.Scale);
            int roundingRadius = (int)(6 * this.Scale);

            RectangleF headerRect = new RectangleF(0, 0, headerWidth, lineHeight);
            RectangleF valueRect = new RectangleF(headerWidth, 0, valueWidth, lineHeight);
            StringFormat headerFormat = new StringFormat() { Alignment = StringAlignment.Near };
            StringFormat valueFormat = new StringFormat() { Alignment = StringAlignment.Center };

            Color accentColor = Color.FromArgb(25, 255, 0, 0);
            CachedBitmap headerBackground = new CachedBitmap(headerWidth, lineHeight, g =>
            {
                Rectangle panelRect = new Rectangle(0, 0, headerWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, 0, 0, roundingRadius);
                g.FillPath(new SolidBrush(Color.FromArgb(225, 10, 10, 10)), path);
                g.DrawLine(new Pen(accentColor), 0 + roundingRadius / 2, lineHeight, headerWidth, lineHeight - 1);
            });
            CachedBitmap valueBackground = new CachedBitmap(valueWidth, lineHeight, g =>
            {
                Rectangle panelRect = new Rectangle(0, 0, valueWidth, lineHeight);
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(panelRect, 0, roundingRadius, 0, 0);
                g.FillPath(new SolidBrush(Color.FromArgb(225, 0, 0, 0)), path);
                g.DrawLine(new Pen(accentColor), 0, lineHeight - 1, valueWidth, lineHeight - 1);
            });

            _damageHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
            _damageValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
            headerRect.Offset(0, lineHeight);
            valueRect.Offset(0, lineHeight);


            if (_config.InfoPanel.TyreSet)
            {
                _tyreSetHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
                _tyreSetValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
                headerRect.Offset(0, lineHeight);
                valueRect.Offset(0, lineHeight);
            }

            if (_config.InfoPanel.FuelPerLap)
            {
                _fuelPerLapHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
                _fuelPerLapValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
                headerRect.Offset(0, lineHeight);
                valueRect.Offset(0, lineHeight);
            }

            if (_config.InfoPanel.ExhaustTemp)
            {
                _exhaustHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
                _exhaustValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
                headerRect.Offset(0, lineHeight);
                valueRect.Offset(0, lineHeight);
            }

            if (_config.InfoPanel.WaterTemp)
            {
                _waterHeader = new PanelText(_font, headerBackground, headerRect) { StringFormat = headerFormat };
                _waterValue = new PanelText(_font, valueBackground, valueRect) { StringFormat = valueFormat };
                headerRect.Offset(0, lineHeight);
                valueRect.Offset(0, lineHeight);
            }

            this.Width = unscaledHeaderWidth + unscaledValueWidth;
            this.Height = (int)(headerRect.Top / this.Scale);
        }

        public override void BeforeStop()
        {
            _font?.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            _damageHeader.Draw(g, "Damage", this.Scale);
            float totalRepairTime = Damage.GetTotalRepairTime(pagePhysics);
            Brush damageBrush = Damage.HasAnyDamage(pagePhysics) ? Brushes.OrangeRed : Brushes.White;
            _damageValue.Brush = damageBrush;
            _damageValue.Draw(g, $"{totalRepairTime:F1}", this.Scale);

            if (_config.InfoPanel.TyreSet)
            {
                _tyreSetHeader.Draw(g, "Tyre Set", this.Scale);
                _tyreSetValue.Draw(g, $"{pageGraphics.currentTyreSet}", this.Scale);
            }

            if (_config.InfoPanel.FuelPerLap)
            {
                _fuelPerLapHeader.Draw(g, "Fuel/Lap", this.Scale);
                float fuelXLap = LapTracker.Instance.Laps.GetAverageFuelUsage(3);
                if (fuelXLap != -1)
                    fuelXLap /= 1000f;
                else fuelXLap = pageGraphics.FuelXLap;
                _fuelPerLapValue.Draw(g, $"{fuelXLap:F3}", this.Scale);
            }

            if (_config.InfoPanel.ExhaustTemp)
            {
                _exhaustHeader.Draw(g, "Exhaust", this.Scale);
                _exhaustValue.Draw(g, $"{pageGraphics.ExhaustTemperature:F0} C", this.Scale);
            }

            if (_config.InfoPanel.WaterTemp)
            {
                _waterHeader.Draw(g, "Water", this.Scale);
                _waterValue.Draw(g, $"{pagePhysics.WaterTemp:F0} C", this.Scale);
            }
        }
    }
}
