using ACC_Manager.Util.NumberExtensions;
using ACCManager.HUD.ACC.Data.Tracker.Laps;
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
            infoPanel = new InfoPanel(10, this.Width - 1);
            this.Height = this.infoPanel.FontHeight * 6;
            RefreshRateHz = 2;
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
            // Some global variants
            double bestLapTime = pageGraphics.BestTimeMs; bestLapTime.ClipMax(180000);
            double fuelTimeLeft = pageGraphics.FuelEstimatedLaps * bestLapTime;
            double stintDebug = pageGraphics.DriverStintTimeLeft; stintDebug.ClipMin(-1);
            //**********************
            // Workings
            double stintFuel = pageGraphics.DriverStintTimeLeft / bestLapTime * pageGraphics.FuelXLap + pageGraphics.UsedFuelSinceRefuel;
            double fuelToEnd = pageGraphics.SessionTimeLeft / bestLapTime * pageGraphics.FuelXLap;
            double fuelToAdd = FuelToAdd(stintDebug, stintFuel, fuelToEnd);
            string fuelTime = $"{TimeSpan.FromMilliseconds(fuelTimeLeft):hh\\:mm\\:ss}";
            string stintTime = $"{TimeSpan.FromMilliseconds(stintDebug):hh\\:mm\\:ss}";
            //**********************
            Brush fuelBarBrush = pagePhysics.Fuel / pageStatic.MaxFuel < 0.15 ? Brushes.Red : Brushes.OrangeRed;
            Brush fuelTimeBrush = GetFuelTimeBrush(fuelTimeLeft, stintDebug);
            //Start (Basic)
            infoPanel.AddProgressBarWithCenteredText($"{pagePhysics.Fuel:F2} L", 0, pageStatic.MaxFuel, pagePhysics.Fuel, fuelBarBrush);
            infoPanel.AddLine("Laps Left", $"{pageGraphics.FuelEstimatedLaps:F1} @ {pageGraphics.FuelXLap:F2}L");
            if (this.config.IncludeFuelBuffer)
                infoPanel.AddLine("Fuel-End+", $"{fuelToEnd + pageGraphics.FuelXLap:F1} : Add {fuelToAdd:F0}");
            else
                infoPanel.AddLine("Fuel-End", $"{fuelToEnd:F1} : Add {fuelToAdd:F0}");
            //End (Basic)
            //Magic Start (Advanced)
            if (this.config.ShowAdvancedInfo)
            {
                infoPanel.AddLine("Stint Time", stintTime);
                infoPanel.AddLine("Fuel Time", fuelTime, fuelTimeBrush);

                if (stintDebug == -1)
                    infoPanel.AddLine("Stint Fuel", "No Stints");
                else
                    if (this.config.IncludeFuelBuffer)
                    infoPanel.AddLine("Stint Fuel+", $"{stintFuel + pageGraphics.FuelXLap:F1}");
                else
                    infoPanel.AddLine("Stint Fuel", $"{stintFuel:F1}");
            }
            //Magic End (Advanced)
            infoPanel.Draw(g);
        }

        private double FuelToAdd(double stintDebug, double stintFuel, double fuelToEnd)
        {
            double fuel;
            if (this.config.IncludeFuelBuffer)
                if (stintDebug == -1)
                    fuel = Math.Min(Math.Ceiling(fuelToEnd - pagePhysics.Fuel), pageStatic.MaxFuel) + pageGraphics.FuelXLap;
                else
                    fuel = Math.Min(stintFuel - pagePhysics.Fuel, pageStatic.MaxFuel) + pageGraphics.FuelXLap;
            else
                if (stintDebug == -1)
                fuel = Math.Min(Math.Ceiling(fuelToEnd - pagePhysics.Fuel), pageStatic.MaxFuel);
            else
                fuel = Math.Min(stintFuel - pagePhysics.Fuel, pageStatic.MaxFuel);
            fuel.ClipMin(0);
            return fuel;
        }

        private Brush GetFuelTimeBrush(double fuelTimeLeft, double stintDebug)
        {
            Brush brush;
            if (stintDebug > -1)
                brush = fuelTimeLeft <= stintDebug ? Brushes.Red : Brushes.LimeGreen;
            else
                brush = fuelTimeLeft <= pageGraphics.SessionTimeLeft ? Brushes.Red : Brushes.LimeGreen;
            return brush;
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