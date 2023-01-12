using RaceElement.Data.ACC.Cars;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayCarInfo
{
    [Overlay(Name = "Car Info", Version = 1.00, OverlayType = OverlayType.Release,
        Description = "A panel showing the damage time. Optionally showing current tyre set, fuel per lap, exhaust temp and water temp.")]
    internal sealed class CarInfoOverlay : AbstractOverlay
    {
        private readonly CarInfoConfiguration _config = new CarInfoConfiguration();
        private class CarInfoConfiguration : OverlayConfiguration
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

            public CarInfoConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private readonly InfoPanel infoPanel;

        public CarInfoOverlay(Rectangle rectangle) : base(rectangle, "Car Info")
        {
            int panelWidth = 140;

            this.infoPanel = new InfoPanel(10, panelWidth);
            this.Width = panelWidth + 1;
            this.Height = this.infoPanel.FontHeight * 5 + 1;
            this.RefreshRateHz = 3;
        }

        public sealed override bool ShouldRender() => DefaultShouldRender();

        public sealed override void BeforeStart()
        {
            if (!this._config.InfoPanel.FuelPerLap)
                this.Height -= this.infoPanel.FontHeight;

            if (!this._config.InfoPanel.ExhaustTemp)
                this.Height -= this.infoPanel.FontHeight;

            if (!this._config.InfoPanel.WaterTemp)
                this.Height -= this.infoPanel.FontHeight;

            if (!this._config.InfoPanel.TyreSet)
                this.Height -= this.infoPanel.FontHeight;
        }

        public sealed override void BeforeStop() { }

        public sealed override void Render(Graphics g)
        {
            float totalRepairTime = Damage.GetTotalRepairTime(pagePhysics);
            Brush damageBrush = Damage.HasAnyDamage(pagePhysics) ? Brushes.OrangeRed : Brushes.White;
            infoPanel.AddLine("Damage", $"{totalRepairTime:F1}", damageBrush);

            if (_config.InfoPanel.TyreSet)
                infoPanel.AddLine("Tyre Set", $"{pageGraphics.currentTyreSet}");

            if (this._config.InfoPanel.FuelPerLap)
            {
                float fuelXLap = LapTracker.Instance.Laps.GetAverageFuelUsage(3);
                if (fuelXLap != -1)
                    fuelXLap /= 1000f;
                else fuelXLap = pageGraphics.FuelXLap;
                infoPanel.AddLine("Fuel/Lap", $"{fuelXLap:F3}");
            }

            if (this._config.InfoPanel.WaterTemp)
                infoPanel.AddLine("Water", $"{pagePhysics.WaterTemp:F0} C");

            if (this._config.InfoPanel.ExhaustTemp)
                infoPanel.AddLine("Exhaust", $"{pageGraphics.ExhaustTemperature:F0} C");

            infoPanel.Draw(g);
        }
    }
}
