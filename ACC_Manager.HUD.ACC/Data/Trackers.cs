using ACCManager.HUD.ACC.Data.Tracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Data.Tracker
{
    public static class Trackers
    {
        public static void StopAll()
        {
            LapTracker.Instance.Stop();
        }
    }
}
