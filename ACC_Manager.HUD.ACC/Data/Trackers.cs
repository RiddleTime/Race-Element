using ACCManager.HUD.ACC.Data.Tracker.Laps;
using ACCManager.HUD.ACC.Data.Tracker.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Data.Tracker
{
    public static class Trackers
    {

        public static void StartAll()
        {
            LapTracker.Instance.ToString();
        }

        public static void StopAll()
        {
            LapTracker.Instance.Stop();
            WeatherTracker.Instance.Stop();
        }
    }
}
