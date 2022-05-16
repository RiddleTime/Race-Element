using ACC_Manager.Util.NumberExtensions;
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
            internal bool IncludeFuelBuffer { get; set; } = true;

            public FuelInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        public FuelInfoOverlay(Rectangle rectangle) : base(rectangle, "Fuel Info Overlay")
        {
            this.Width = 240;
            this.Height = 120;// 120;
            infoPanel = new InfoPanel(10, this.Width - 1);
            RefreshRateHz = 5;
        }


        public override void BeforeStart()
        {
            if (!this.config.ShowAdvancedInfo)
            {
                this.Height -= this.infoPanel.FontHeight * 3;
            }

            if (!this.config.IncludeFuelBuffer)
            {
                this.Height += this.infoPanel.FontHeight;
            }
        }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            double bestLapTime = pageGraphics.BestTimeMs; bestLapTime.ClipMax(80000);
            double fuelInCarDebug = Math.Max(pagePhysics.Fuel, 0);
            double stintDebug = pageGraphics.DriverStintTimeLeft < 0 ? 0 : pageGraphics.DriverStintTimeLeft;

            TimeSpan time = TimeSpan.FromMilliseconds(stintDebug);
            string stintTime = time.ToString(@"hh\:mm\:ss");

            double fuelToEnd = pageGraphics.SessionTimeLeft / bestLapTime * pageGraphics.FuelXLap;
            double fuelToEndBuffer = pageGraphics.SessionTimeLeft / bestLapTime * pageGraphics.FuelXLap + pageGraphics.FuelXLap;
            double fuelToAdd = Math.Max(Math.Min(Math.Ceiling(fuelToEnd - fuelInCarDebug), pageStatic.MaxFuel), 0);
            double stintFuelBuffer = pageGraphics.DriverStintTimeLeft / bestLapTime * pageGraphics.FuelXLap + pageGraphics.UsedFuelSinceRefuel + pageGraphics.FuelXLap;
            double stintFuel = pageGraphics.DriverStintTimeLeft / bestLapTime * pageGraphics.FuelXLap + pageGraphics.UsedFuelSinceRefuel;

            double fuelTimeCalc = (long)(fuelInCarDebug / pageGraphics.FuelXLap) * bestLapTime;
            TimeSpan time2 = TimeSpan.FromMilliseconds(fuelTimeCalc);
            string fuelTime = time2.ToString(@"hh\:mm\:ss");

            Brush fuelBarBrush = pagePhysics.Fuel / pageStatic.MaxFuel < 0.15 ? Brushes.Red : Brushes.OrangeRed;
            //Start (Basic)
            infoPanel.AddProgressBarWithCenteredText($"{pagePhysics.Fuel:F2} L", 0, pageStatic.MaxFuel, pagePhysics.Fuel, fuelBarBrush);
            infoPanel.AddLine("Laps Left", $"{ pageGraphics.FuelEstimatedLaps.ToString("F1")} : {pageGraphics.FuelXLap}L");
            if(this.config.IncludeFuelBuffer)
            infoPanel.AddLine("Fuel-End+", $"{fuelToEndBuffer.ToString("F1")} : Add {Math.Min(fuelToAdd, stintFuelBuffer).ToString("F0")}");
            else
            infoPanel.AddLine("Fuel-End", $"{fuelToEnd.ToString("F1")} : Add {Math.Min(fuelToAdd, stintFuel).ToString("F0")}");
            //End (Basic)
            //Magic Start (Advanced)
            if (this.config.ShowAdvancedInfo)
            {
                infoPanel.AddLine("Stint Time", stintTime);
                infoPanel.AddLine("Fuel Time", fuelTime);
                if (this.config.IncludeFuelBuffer)
                infoPanel.AddLine("Stint Fuel+", stintFuelBuffer.ToString("F1"));   
                else
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
    }
}