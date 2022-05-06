using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.ACCSharedMemory;
using static ACCSetupApp.Controls.HUD.Overlay.OverlayUtil.InfoPanel;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayTrackInfo
{
    internal class FuelInfoOverlay : AbstractOverlay
    {
        private readonly InfoPanel panel = new InfoPanel(12);

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
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

             

            double fuelPercent = (pagePhysics.Fuel / pageStatic.MaxFuel) * 100;
            double lapsOfFuel = pagePhysics.Fuel / pageGraphics.FuelXLap;
            double lapsToEnd = pageGraphics.SessionTimeLeft / pageGraphics.BestTimeMs;
            double fuelToEnd = (pageGraphics.SessionTimeLeft / pageGraphics.BestTimeMs) * pageGraphics.FuelXLap + pageGraphics.FuelXLap;
            double fuelToAdd = Math.Round((fuelToEnd + 0.5) - pagePhysics.Fuel);

            panel.AddLine("Fuel", $"{(pagePhysics.Fuel).ToString("F2")}ltr : {fuelPercent.ToString("F1")}%");
            panel.AddLine("Max Fuel", pageStatic.MaxFuel.ToString("F0"));
            panel.AddLine("Laps Fuel", lapsOfFuel.ToString("F1"));
            panel.AddLine("Fuel-End", $"{fuelToEnd.ToString("F1")} : Add {fuelToAdd.ToString("F0")}ltr");


            panel.AddLine("fuelxlap", pageGraphics.FuelXLap.ToString("F1"));
            panel.AddLine("fuel", pagePhysics.Fuel.ToString("F1"));
            //panel.AddLine("Flag", FlagTypeToString(pageGraphics.Flag));
            //panel.AddLine("Session", SessionTypeToString(pageGraphics.SessionType));
            //panel.AddLine("Grip", pageGraphics.trackGripStatus.ToString());
            //string airTemp = Math.Round(pagePhysics.AirTemp, 2).ToString("F2");
            //string roadTemp = Math.Round(pagePhysics.RoadTemp, 2).ToString("F2");
            //panel.AddLine("Air - Track", $"{airTemp} C - {roadTemp} C");
            //panel.AddLine("Wind", $"{Math.Round(pageGraphics.WindSpeed, 2)} km/h");
            panel.Draw(g);
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }


    }
}
