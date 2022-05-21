using ACCManager.Data;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayEcuMapInfo
{
    internal sealed class EcuMapOverlay : AbstractOverlay
    {
        private const int PanelWidth = 270;
        InfoPanel Panel = new InfoPanel(10, PanelWidth);

        private EcuMapConfiguration config = new EcuMapConfiguration();
        private class EcuMapConfiguration : OverlayConfiguration
        {
            public EcuMapConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        public EcuMapOverlay(Rectangle rectangle) : base(rectangle, "Ecu Maps Overlay")
        {
            this.RefreshRateHz = 4;

            this.Width = PanelWidth + 1;
            this.Height = Panel.FontHeight * 5 + 1;
        }

        public override void BeforeStart() { }
        public override void BeforeStop() { }

        public override void Render(Graphics g)
        {
            EcuMap current = EcuMaps.GetMap(pageStatic.CarModel, pageGraphics.EngineMap);

            if (current != null)
            {
                Panel.AddLine("Map", $"{current.Index}");
                Panel.AddLine("Power", $"{current.Power}");
                Panel.AddLine("Condition", $"{current.Conditon}");
                Panel.AddLine("Fuel", $"{current.FuelConsumption}");
                Panel.AddLine("Throttle", $"{current.ThrottleMap}");
            }
            else
            {
                Panel.AddLine("Car", "Not supported");
                Panel.AddLine("Model", pageStatic.CarModel);
                Panel.AddLine("Map", $"{pageGraphics.EngineMap + 1}");
            }

            Panel.Draw(g);
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
