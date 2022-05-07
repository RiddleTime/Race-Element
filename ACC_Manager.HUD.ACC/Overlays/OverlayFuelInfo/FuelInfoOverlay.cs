using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayFuelInfo
{
    internal class FuelInfoOverlay : AbstractOverlay
    {
        private readonly InfoPanel panel = new InfoPanel(12, 350);

        public FuelInfoOverlay(Rectangle rectangle) : base(rectangle, "Fuel Info Overlay")
        {
            this.Width = 350;
            this.Height = 150;
            RefreshRateHz = 5;
        }

        public override void BeforeStart() { }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(pageGraphics.DriverStintTimeLeft);
            string stintTime = time.ToString(@"hh\:mm\:ss");

            double fuelPercent = (pagePhysics.Fuel / pageStatic.MaxFuel) * 100;
            double lapsOfFuel = pagePhysics.Fuel / pageGraphics.FuelXLap;
            double fuelToEnd = (pageGraphics.SessionTimeLeft / pageGraphics.BestTimeMs) * pageGraphics.FuelXLap + pageGraphics.FuelXLap;
            double fuelToAdd = Math.Max(Math.Min(Math.Ceiling(fuelToEnd - pagePhysics.Fuel), pageStatic.MaxFuel), 0);

            panel.AddProgressBarWithCenteredText($"Fuel: {(fuelPercent):F1}%", 0, pagePhysics.Fuel, pageStatic.MaxFuel);
            //panel.AddLine("Fuel", $"{pagePhysics.Fuel.ToString("F1")} : {fuelPercent.ToString("F1")}%");
            panel.AddLine("Laps Fuel", lapsOfFuel.ToString("F1"));
            panel.AddLine("Fuel-End", $"{fuelToEnd.ToString("F1")} : Add {fuelToAdd.ToString("F0")}");

            panel.AddLine("Stint Time", stintTime);

            panel.Draw(g);
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            bool shouldRender = true;
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }

    }
}