using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD
{
    public class HUDDisposer
    {
        public static void Dispose()
        {
            BroadcastTracker.Instance.Dispose();
        }
    }
}
