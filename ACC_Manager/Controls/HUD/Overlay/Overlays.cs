using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.HUD.Overlay.OverlayCarDamage;
using ACCSetupApp.Controls.HUD.Overlay.OverlayEcuMapInfo;
using ACCSetupApp.Controls.HUD.Overlay.OverlayGraphicsInfo;
using ACCSetupApp.Controls.HUD.Overlay.OverlayPhysicsInfo;
using ACCSetupApp.Controls.HUD.Overlay.OverlayPressureTrace;
using ACCSetupApp.Controls.HUD.Overlay.OverlayStaticInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay
{
    internal class Overlays
    {
        internal static Dictionary<string, Type> AbstractOverlays = new Dictionary<string, Type>()
        {
            {"ECU Maps", typeof(EcuMapOverlay) },
            {"Input trace", typeof(InputTraceOverlay) },
            {"Tire Pressure Trace", typeof(PressureTraceOverlay) },
            //{"Repair Time", typeof(CarDamageOverlay) },
            {"Debug Static Data", typeof(StaticInfoOverlay) },
            {"Debug Physics Data", typeof(PhysicsInfoOverlay) },
            {"Debug Graphics Data", typeof(GraphicsInfoOverlay) }

        };

        internal static List<AbstractOverlay> ActiveOverlays = new List<AbstractOverlay>();

        public static void CloseAll()
        {
            lock (ActiveOverlays)
                while (Overlays.ActiveOverlays.Count > 0)
                {
                    Overlays.ActiveOverlays.ElementAt(0).Stop();
                    Overlays.ActiveOverlays.ElementAt(0).Dispose();
                    ActiveOverlays.Remove(Overlays.ActiveOverlays.ElementAt(0));
                }
        }
    }
}
