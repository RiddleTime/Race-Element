using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.HUD.ACC.Overlays.OverlayCarInfo
{
    internal class CarInfoOverlay : AbstractOverlay
    {
        InfoPanel infoPanel = new InfoPanel(10, 300);
        private float MagicDamageMultiplier = 0.282f;

        public CarInfoOverlay(Rectangle rectangle) : base(rectangle, "Car Info Overlay")
        {
            this.Width = 300;
            this.Height = 200;
            this.RefreshRateHz = 10;
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop()
        {
        }

        public override void Render(Graphics g)
        {
            float totalRepairTime = GetTotalRepairTime();
            infoPanel.AddLine("Repair time", $"{totalRepairTime}");
            infoPanel.AddLine("Tyre set", $"{pageGraphics.currentTyreSet}");
            infoPanel.AddLine("Fuel per lap", $"{Math.Round(pageGraphics.FuelXLap, 3)}");
            infoPanel.AddProgressBar($"Fuel:", 0, pagePhysics.Fuel, pageStatic.MaxFuel);


            infoPanel.Draw(g);
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            return HasAnyDamage();
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
