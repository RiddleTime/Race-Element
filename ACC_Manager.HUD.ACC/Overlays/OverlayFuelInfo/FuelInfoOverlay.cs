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
    internal sealed class FuelInfoOverlay : AbstractOverlay
    {
        InfoPanel infoPanel;

        private FuelInfoConfig config = new FuelInfoConfig();
        private class FuelInfoConfig : OverlayConfiguration
        {
            internal bool ShowAdvancedInfo { get; set; } = true;

            public FuelInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        public FuelInfoOverlay(Rectangle rectangle) : base(rectangle, "Fuel Info Overlay")
        {
            this.Width = 240;
            this.Height = 105;// 120;
            infoPanel = new InfoPanel(10, this.Width - 1);
            RefreshRateHz = 5;
        }


        public override void BeforeStart()
        {
            if (!this.config.ShowAdvancedInfo)
            {
                this.Height -= this.infoPanel.FontHeight * 3;
            }
        }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            double laptimePlaceholder = LaptimePlaceholder2();
            double fuelInCarDebug = Math.Max(pagePhysics.Fuel, 1);

            TimeSpan time = TimeSpan.FromMilliseconds(pageGraphics.DriverStintTimeLeft);
            string stintTime = time.ToString(@"hh\:mm\:ss");

            double fuelToEnd = pageGraphics.SessionTimeLeft / laptimePlaceholder * pageGraphics.FuelXLap + pageGraphics.FuelXLap;
            double fuelToAdd = Math.Max(Math.Min(Math.Ceiling(fuelToEnd - fuelInCarDebug), pageStatic.MaxFuel), 0);
            double stintFuel = pageGraphics.DriverStintTimeLeft / laptimePlaceholder * pageGraphics.FuelXLap + pageGraphics.UsedFuelSinceRefuel + 1;

            double fuelTimeCalc = (long)(fuelInCarDebug / pageGraphics.FuelXLap) * laptimePlaceholder;
            TimeSpan time2 = TimeSpan.FromMilliseconds(fuelTimeCalc);
            string fuelTime = time2.ToString(@"hh\:mm\:ss");

            //Start (Basic)
            infoPanel.AddProgressBarWithCenteredText($"{pagePhysics.Fuel:F2} L", 0, pageStatic.MaxFuel, pagePhysics.Fuel);
            infoPanel.AddLine("Laps Left", pageGraphics.FuelEstimatedLaps.ToString("F1"));
            infoPanel.AddLine("Fuel-End", $"{fuelToEnd.ToString("F1")} : Add {fuelToAdd.ToString("F0")}");
            //End (Basic)
            //Magic Start (Advanced)
            if (this.config.ShowAdvancedInfo)
            {
                infoPanel.AddLine("Stint Time", stintTime);
                infoPanel.AddLine("Fuel Time", fuelTime);
                infoPanel.AddLine("Stint Fuel", stintFuel.ToString("F1"));
            }
            //Magic End (Advanced)
            infoPanel.Draw(g);
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

        private double LaptimePlaceholder2()
        {
            double endResult = 0.0;
            if (pageGraphics.BestTimeMs > 480000)
            {
                endResult = 80000;
            }
            else
            {
                endResult = pageGraphics.BestTimeMs;
            }
            return endResult;
        }

    }
}