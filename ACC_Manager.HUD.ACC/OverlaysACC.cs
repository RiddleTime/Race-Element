using ACCManager.HUD.ACC.Overlays.OverlayAccelerometer;
using ACCManager.HUD.ACC.Overlays.OverlayCarInfo;
using ACCManager.HUD.ACC.Overlays.OverlayEcuMapInfo;
using ACCManager.HUD.ACC.Overlays.OverlayGraphicsInfo;
using ACCManager.HUD.ACC.Overlays.OverlayInputTrace;
using ACCManager.HUD.ACC.Overlays.OverlayPhysicsInfo;
using ACCManager.HUD.ACC.Overlays.OverlayPressureTrace;
using ACCManager.HUD.ACC.Overlays.OverlayStaticInfo;
using ACCManager.HUD.ACC.Overlays.OverlayTrackInfo;
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
            {"ECU Maps", typeof(EcuMapOverlay) },
            {"Input trace", typeof(InputTraceOverlay) },
            {"Tire Pressure Trace", typeof(PressureTraceOverlay) },

            {"Track Info Overlay", typeof(TrackInfoOverlay) },

#if DEBUG
            {"Accelerometer", typeof(AccelerometerOverlay) },
            {"Car Info Overlay", typeof(CarInfoOverlay) },
            {"Fuel Info Overlay", typeof(FuelInfoOverlay) },
          

#endif


            // yea this shit has to be at the bottom...
            {"Debug Static Data", typeof(StaticInfoOverlay) },
            {"Debug Physics Data", typeof(PhysicsInfoOverlay) },
            {"Debug Graphics Data", typeof(GraphicsInfoOverlay) }

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
