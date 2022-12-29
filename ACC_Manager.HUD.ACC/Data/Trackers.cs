using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.ACC.Data.Tracker.Weather;

namespace RaceElement.HUD.ACC.Data.Tracker
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
