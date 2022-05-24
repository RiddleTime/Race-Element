using ACCManager.Data.ACC.Tracker;
using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD
{
    public class HUDDisposer
    {
        public static void Dispose()
        {
            Debug.WriteLine("Safely disposing HUD instances");
            BroadcastTracker.Instance.Dispose();
        }
    }
}
