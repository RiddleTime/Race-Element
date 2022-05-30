using ACCManager.Data;
using ACCManager.Data.ACC.Session;
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
        private readonly InfoPanel _panel = new InfoPanel(10, PanelWidth);

        private EcuMapConfiguration _config = new EcuMapConfiguration();
        private class EcuMapConfiguration : OverlayConfiguration
        {
            [ToolTip("Displays the number of the ecu map.")]
            public bool ShowMapNumber { get; set; } = true;

            public EcuMapConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        public EcuMapOverlay(Rectangle rectangle) : base(rectangle, "Ecu Maps Overlay")
        {
            this.RefreshRateHz = 4;

            this.Width = PanelWidth + 1;
            this.Height = _panel.FontHeight * 5 + 1;
        }

        public sealed override void BeforeStart()
        {
            if (!this._config.ShowMapNumber)
            {
                this.Height -= _panel.FontHeight;
            }
        }
        public sealed override void BeforeStop() { }

        public sealed override void Render(Graphics g)
        {
            EcuMap current = EcuMaps.GetMap(pageStatic.CarModel, pageGraphics.EngineMap);

            if (current != null)
            {
                if (this._config.ShowMapNumber)
                    _panel.AddLine("Map", $"{current.Index}");
                _panel.AddLine("Power", $"{current.Power}");
                _panel.AddLine("Condition", $"{current.Conditon}");
                _panel.AddLine("Fuel", $"{current.FuelConsumption}");
                _panel.AddLine("Throttle", $"{current.ThrottleMap}");
            }
            else
            {
                _panel.AddLine("Car", "Not supported");
                _panel.AddLine("Model", pageStatic.CarModel);
                _panel.AddLine("Map", $"{pageGraphics.EngineMap + 1}");
            }

            _panel.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            bool shouldRender = true;
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            if (pageGraphics.GlobalRed)
                shouldRender = false;

            if (RaceSessionState.IsPreSession(pageGraphics.GlobalRed, broadCastRealTime.Phase))
                shouldRender = true;

            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE)
                shouldRender = false;

            return shouldRender;
        }
    }
}
