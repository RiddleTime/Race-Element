using ACC_Manager.Util.NumberExtensions;
using ACCManager.HUD.ACC.Data.Tracker.Laps;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.HUD.ACC.Overlays.OverlayCarInfo
{
    internal sealed class CarInfoOverlay : AbstractOverlay
    {
        private CarInfoConfiguration config = new CarInfoConfiguration();
        private class CarInfoConfiguration : OverlayConfiguration
        {
            [ToolTip("Displays the average fuel usage over the past 3 laps.\n(uses last lap info if not enough data)")]
            public bool ShowAverageFuelUsage { get; set; } = false;

            [ToolTip("Displays the exhaust temperature.")]
            public bool ShowExhaustTemp { get; set; } = false;

            [ToolTip("Displays the water temperature of the engine.")]
            public bool ShowWaterTemp { get; set; } = false;

            public CarInfoConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private readonly InfoPanel infoPanel;
        private const float MagicDamageMultiplier = 0.282f;

        public CarInfoOverlay(Rectangle rectangle) : base(rectangle, "Car Info Overlay")
        {
            int panelWidth = 180;

            this.infoPanel = new InfoPanel(10, panelWidth);
            this.Width = panelWidth + 1;
            this.Height = this.infoPanel.FontHeight * 5 + 1;
            this.RefreshRateHz = 3;
        }

        public sealed override void BeforeStart()
        {
            if (!this.config.ShowAverageFuelUsage)
                this.Height -= this.infoPanel.FontHeight;

            if (!this.config.ShowExhaustTemp)
                this.Height -= this.infoPanel.FontHeight;

            if (!this.config.ShowWaterTemp)
                this.Height -= this.infoPanel.FontHeight;
        }

        public sealed override void BeforeStop()
        {
        }

        public sealed override void Render(Graphics g)
        {
            float totalRepairTime = GetTotalRepairTime();
            Brush repairBrush = HasAnyDamage() ? Brushes.Red : Brushes.White;
            infoPanel.AddLine("Repair Time", $"{totalRepairTime:F1}", repairBrush);
            infoPanel.AddLine("Tyre Set", $"{pageGraphics.currentTyreSet}");

            if (this.config.ShowAverageFuelUsage)
            {
                float fuelXLap = LapTracker.Instance.Laps.GetAverageFuelUsage(3);
                if (fuelXLap != -1)
                    fuelXLap /= 1000f;
                else fuelXLap = pageGraphics.FuelXLap;
                infoPanel.AddLine("Av. Fuel/lap", $"{fuelXLap:F3}");
            }

            if (this.config.ShowWaterTemp)
                infoPanel.AddLine("Water temp", $"{pagePhysics.WaterTemp:F0} C");

            if (this.config.ShowExhaustTemp)
                infoPanel.AddLine("Exhaust temp", $"{pageGraphics.ExhaustTemperature:F0} C");

            infoPanel.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }

        private float GetTotalRepairTime()
        {
            float totalRepairTime = 0;

            totalRepairTime += GetBodyWorkDamage(CarDamagePosition.Centre);

            foreach (Wheel wheel in Enum.GetValues(typeof(Wheel)))
                totalRepairTime += GetSuspensionDamage(wheel);

            return totalRepairTime;
        }

        private bool HasAnyDamage()
        {
            foreach (int i in Enum.GetValues(typeof(CarDamagePosition)))
                if (pagePhysics.CarDamage[i] > 0)
                    return true;

            foreach (int i in Enum.GetValues(typeof(Wheel)))
                if (pagePhysics.SuspensionDamage[i] > 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Gets the amount of damage/repair-time for the given wheel
        /// </summary>
        /// <param name="wheel"></param>
        /// <returns></returns>
        private float GetSuspensionDamage(Wheel wheel)
        {
            return pagePhysics.SuspensionDamage[(int)wheel] * 30;
        }

        /// <summary>
        /// Gets the amount of bodywork damage/repair-time for the given car damage position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private float GetBodyWorkDamage(CarDamagePosition position)
        {
            return pagePhysics.CarDamage[(int)position] * MagicDamageMultiplier;
        }

        private enum CarDamagePosition : int
        {
            Front,
            Rear,
            Left,
            Right,
            Centre
        }
    }
}
