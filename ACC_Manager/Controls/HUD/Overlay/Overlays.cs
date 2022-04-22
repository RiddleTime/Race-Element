using ACCSetupApp.Controls.HUD.Overlay.Internal;
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
            {"Input trace", typeof(InputTraceOverlay) },
            {"Debug Static Data", typeof(StaticInfoOverlay) },
            {"Tire Pressure Trace", typeof(PressureTraceOverlay) }
        };

        internal static List<AbstractOverlay> ActiveOverlays = new List<AbstractOverlay>();
    }
}
