using ACC_Manager.Util.NumberExtensions;
using ACCManager.HUD.Overlay.Configuration;
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
        private readonly InfoPanel _infoPanel;

        private readonly FuelInfoConfig _config = new FuelInfoConfig();
        private class FuelInfoConfig : OverlayConfiguration
        {
            [ToolTip("Basic: Displays fuel bar, laps left and fuel-to-end information." +
                    "\nAdvanced: Additionally displays stint time remaining, fuel time remaining and the suggested amount" +
                    "\nof fuel needed to end the stint or session." +
                    "\nFuel time remaining is green if it's higher than stint time or session time and red if it is not.")]
            public bool ShowAdvancedInfo { get; set; } = true;

            [ToolTip("Sets the number of additional laps as a fuel buffer.")]
            [IntRange(0, 3, 1)]
            public int FuelBufferLaps { get; set; } = 0;

            public FuelInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        public FuelInfoOverlay(Rectangle rectangle) : base(rectangle, "Fuel Info Overlay")
        {
            this.Width = 220;
            _infoPanel = new InfoPanel(10, this.Width - 1) { FirstRowLine = 1 };
            this.Height = this._infoPanel.FontHeight * 6 + 1;
            RefreshRateHz = 2;
        }

        public sealed override void BeforeStart()
        {
            if (!this._config.ShowAdvancedInfo)
                this.Height -= this._infoPanel.FontHeight * 3;
        }

        public sealed override void BeforeStop() { }

        public sealed override void Render(Graphics g)
        {
            // Some global variants
            double lapBufferVar = pageGraphics.FuelXLap * this._config.FuelBufferLaps;
            double bestLapTime = pageGraphics.BestTimeMs; bestLapTime.ClipMax(180000);
            double fuelTimeLeft = pageGraphics.FuelEstimatedLaps * bestLapTime;
            double stintDebug = pageGraphics.DriverStintTimeLeft; stintDebug.ClipMin(-1);
            //**********************
            // Workings
            double stintFuel = pageGraphics.DriverStintTimeLeft / bestLapTime * pageGraphics.FuelXLap + pageGraphics.UsedFuelSinceRefuel;
            double fuelToEnd = pageGraphics.SessionTimeLeft / bestLapTime * pageGraphics.FuelXLap;
            double fuelToAdd = FuelToAdd(lapBufferVar, stintDebug, stintFuel, fuelToEnd);
            string fuelTime = $"{TimeSpan.FromMilliseconds(fuelTimeLeft):hh\\:mm\\:ss}";
            string stintTime = $"{TimeSpan.FromMilliseconds(stintDebug):hh\\:mm\\:ss}";
            //**********************
            Brush fuelBarBrush = pagePhysics.Fuel / pageStatic.MaxFuel < 0.15 ? Brushes.Red : Brushes.OrangeRed;
            Brush fuelTimeBrush = GetFuelTimeBrush(fuelTimeLeft, stintDebug);
            //Start (Basic)
            pageStatic.MaxFuel = 50;
            _infoPanel.AddProgressBarWithCenteredText($"{pagePhysics.Fuel:F2} L", 0, pageStatic.MaxFuel, pagePhysics.Fuel, fuelBarBrush);
            _infoPanel.AddLine("Laps Left", $"{pageGraphics.FuelEstimatedLaps:F1} @ {pageGraphics.FuelXLap:F2}L");
            _infoPanel.AddLine("Fuel-End", $"{fuelToEnd + lapBufferVar:F1} : Add {fuelToAdd:F0}");
            //End (Basic)
            //Magic Start (Advanced)
            if (this._config.ShowAdvancedInfo)
            {
                _infoPanel.AddLine("Stint Time", stintTime);
                _infoPanel.AddLine("Fuel Time", fuelTime, fuelTimeBrush);

                if (stintDebug == -1)
                    _infoPanel.AddLine("Stint Fuel", "No Stints");
                else
                    _infoPanel.AddLine("Stint Fuel", $"{stintFuel + lapBufferVar:F1}");
            }
            //Magic End (Advanced)
            _infoPanel.Draw(g);
        }

        private double FuelToAdd(double lapBufferVar, double stintDebug, double stintFuel, double fuelToEnd)
        {
            double fuel;
            if (stintDebug == -1)
                fuel = Math.Min(Math.Ceiling(fuelToEnd - pagePhysics.Fuel), pageStatic.MaxFuel) + lapBufferVar;
            else
                fuel = Math.Min(stintFuel - pagePhysics.Fuel, pageStatic.MaxFuel) + lapBufferVar;
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

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}