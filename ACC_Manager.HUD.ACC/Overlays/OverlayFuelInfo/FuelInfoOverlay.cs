﻿using ACCManager.HUD.Overlay.Internal;
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
        InfoPanel panel = new InfoPanel(12, 500);

        public FuelInfoOverlay(Rectangle rectangle) : base(rectangle, "Fuel Info Overlay")
        {
            this.Width = 260;
            this.Height = 120;// 120;
            RefreshRateHz = 5;
        }


        public override void BeforeStart() { }

        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            double laptimePlaceholder = LaptimePlaceholder2();
            double fuelInCarDebug = Math.Max(pagePhysics.Fuel, 1);

            TimeSpan time = TimeSpan.FromMilliseconds(pageGraphics.DriverStintTimeLeft);
            string stintTime = time.ToString(@"hh\:mm\:ss");

            double fuelPercent = pagePhysics.Fuel / pageStatic.MaxFuel * 100;
            double fuelToEnd = pageGraphics.SessionTimeLeft / laptimePlaceholder * pageGraphics.FuelXLap + pageGraphics.FuelXLap;
            double fuelToAdd = Math.Max(Math.Min(Math.Ceiling(fuelToEnd - fuelInCarDebug), pageStatic.MaxFuel), 0);
            double stintFuel = pageGraphics.DriverStintTimeLeft / laptimePlaceholder * pageGraphics.FuelXLap + pageGraphics.UsedFuelSinceRefuel + 1;

            double fuelTimeCalc = (long)(fuelInCarDebug / pageGraphics.FuelXLap) * laptimePlaceholder;
            TimeSpan time2 = TimeSpan.FromMilliseconds(fuelTimeCalc);
            string fuelTime = time2.ToString(@"hh\:mm\:ss");

            //Start (Basic)
            panel.AddLine("Fuel", $"{pagePhysics.Fuel.ToString("F1")} : {fuelPercent.ToString("F1")}%");
            panel.AddLine("Laps Fuel", pageGraphics.FuelEstimatedLaps.ToString("F1"));
            panel.AddLine("Fuel-End", $"{fuelToEnd.ToString("F1")} : Add {fuelToAdd.ToString("F0")}");
            //End (Basic)
            //Magic Start (Advanced)
            panel.AddLine("Stint Time", stintTime);
            panel.AddLine("Fuel Time", fuelTime);
            panel.AddLine("Stint Fuel", stintFuel.ToString("F1"));
            //Magic End (Advanced)
            //Debug start
            //panel.AddLine("", "");

            /*panel.AddLine("laptime", laptimePlaceholder.ToString("F0"));
            panel.AddLine("Fuel X Lap", pageGraphics.FuelXLap.ToString("F2"));
            //panel.AddLine("Debug Name 1", fuelTimeCalc.ToString("F0"));
            //panel.AddLine("Debug Name 2", fuelTime);
            panel.AddLine("Driver Stint", pageGraphics.DriverStintTimeLeft.ToString("F0"));
            //Debug End*/

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