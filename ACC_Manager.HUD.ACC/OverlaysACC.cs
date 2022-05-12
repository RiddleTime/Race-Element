using ACCManager.HUD.ACC.Overlays.OverlayAccelerometer;
using ACCManager.HUD.ACC.Overlays.OverlayCarInfo;
using ACCManager.HUD.ACC.Overlays.OverlayEcuMapInfo;
using ACCManager.HUD.ACC.Overlays.OverlayFuelInfo;
using ACCManager.HUD.ACC.Overlays.OverlayGraphicsInfo;
using ACCManager.HUD.ACC.Overlays.OverlayInputTrace;
using ACCManager.HUD.ACC.Overlays.OverlayLapDelta;
using ACCManager.HUD.ACC.Overlays.OverlayPhysicsInfo;
using ACCManager.HUD.ACC.Overlays.OverlayPressureTrace;
using ACCManager.HUD.ACC.Overlays.OverlayStaticInfo;
using ACCManager.HUD.ACC.Overlays.OverlayTrackInfo;
using ACCManager.HUD.ACC.Overlays.OverlayTyreInfo;

using ACCManager.HUD.Overlay.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC
{
    public class OverlaysACC
    {
        public static Dictionary<string, Type> AbstractOverlays = new Dictionary<string, Type>()
        {
            {"Accelerometer", typeof(AccelerometerOverlay) },
            {"ECU Maps", typeof(EcuMapOverlay) },
            {"Fuel Info", typeof(FuelInfoOverlay) },
            {"Input Trace", typeof(InputTraceOverlay) },
            {"Lap Delta", typeof(LapDeltaOverlay) },
            {"Track Info", typeof(TrackInfoOverlay) },
            {"Tyre Pressure Trace", typeof(PressureTraceOverlay) },

#if DEBUG
            {"Car Info", typeof(CarInfoOverlay) },
            {"Tyre Info", typeof(TyreInfoOverlay) },


            // yea this shit has to be at the bottom...
            {"Debug Static Data", typeof(StaticInfoOverlay) },
            {"Debug Physics Data", typeof(PhysicsInfoOverlay) },
            {"Debug Graphics Data", typeof(GraphicsInfoOverlay) }
#endif
        };

        public static List<AbstractOverlay> ActiveOverlays = new List<AbstractOverlay>();

        public static void CloseAll()
        {
            lock (ActiveOverlays)
                while (ActiveOverlays.Count > 0)
                {
                    ActiveOverlays.ElementAt(0).Stop();
                    ActiveOverlays.ElementAt(0).Dispose();
                    ActiveOverlays.Remove(ActiveOverlays.ElementAt(0));
                }
        }
    }
}
