using ACCManager.HUD.ACC.Overlays.OverlayAccelerometer;
using ACCManager.HUD.ACC.Overlays.OverlayCarInfo;
using ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayBroadcastRealtime;
using ACCManager.HUD.ACC.Overlays.OverlayEcuMapInfo;
using ACCManager.HUD.ACC.Overlays.OverlayFuelInfo;
using ACCManager.HUD.ACC.Overlays.OverlayGraphicsInfo;
using ACCManager.HUD.ACC.Overlays.OverlayInputTrace;
using ACCManager.HUD.ACC.Overlays.OverlayLapDelta;
using ACCManager.HUD.ACC.Overlays.OverlayPhysicsInfo;
using ACCManager.HUD.ACC.Overlays.OverlayPressureTrace;
using ACCManager.HUD.ACC.Overlays.OverlayStaticInfo;
using ACCManager.HUD.ACC.Overlays.OverlayInputs;
using ACCManager.HUD.ACC.Overlays.OverlayTrackInfo;
using ACCManager.HUD.ACC.Overlays.OverlayTyreInfo;
using ACCManager.HUD.ACC.Overlays.OverlayWeather;
using ACCManager.HUD.Overlay.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayEntryList;
using ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayDebugOutput;

namespace ACCManager.HUD.ACC
{
    public class OverlaysACC
    {
        public static Dictionary<string, Type> AbstractOverlays = new Dictionary<string, Type>()
        {
            {"Accelerometer", typeof(AccelerometerOverlay) },
            {"Car Info", typeof(CarInfoOverlay) },
            {"ECU Maps", typeof(EcuMapOverlay) },
            {"Fuel Info", typeof(FuelInfoOverlay) },
            {"Inputs", typeof(InputsOverlay) },
            {"Input Trace", typeof(InputTraceOverlay) },
            {"Lap Delta", typeof(LapDeltaOverlay) },
            {"Track Info", typeof(TrackInfoOverlay) },
            {"Tyre Info", typeof(TyreInfoOverlay) },
            {"Tyre Pressure Trace", typeof(PressureTraceOverlay) },

#if DEBUG
            {"Weather Info", typeof(WeatherOverlay) },

            // yea this shit has to be at the bottom...
            {"Debug Output", typeof(DebugOutputOverlay) },
            {"Page Static", typeof(StaticInfoOverlay) },
            {"Page Physics", typeof(PhysicsInfoOverlay) },
            {"Page Graphics", typeof(GraphicsInfoOverlay) },
            {"Broadcast Entrylist", typeof(EntryListOverlay) },
            {"Broadcast Realtime", typeof(BroadcastRealtimeOverlay) },
            {"Broadcast Trackdata", typeof(BroadcastTrackDataOverlay) },
#endif
        };

        public static List<AbstractOverlay> ActiveOverlays = new List<AbstractOverlay>();

        public static void CloseAll()
        {
            lock (ActiveOverlays)
                while (ActiveOverlays.Count > 0)
                {
                    ActiveOverlays.ElementAt(0).EnableReposition(false);
                    ActiveOverlays.ElementAt(0).Stop();
                    ActiveOverlays.ElementAt(0).Dispose();
                    ActiveOverlays.Remove(ActiveOverlays.ElementAt(0));
                }
        }
    }
}
