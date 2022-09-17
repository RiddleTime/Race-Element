using ACCManager.Data.ACC.Tracker.Laps;
using ACCManager.HUD.ACC.Data.Tracker.Weather;

namespace ACCManager.HUD.ACC.Data.Tracker
{
    public static class HudTrackers
    {

        public static void StartAll()
        {
            _ = LapTracker.Instance;
        }

        public static void StopAll()
        {
            WeatherTracker.Instance.Stop();
        }
    }
}
