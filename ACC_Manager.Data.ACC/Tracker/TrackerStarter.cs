using ACCManager.Data.ACC.Session;
using ACCManager.Data.ACC.Tracker.Laps;
using System.Diagnostics;
using System.Threading;

namespace ACCManager.Data.ACC.Tracker
{
    public class ACCTrackerStarter
    {
        public static void StartACCTrackers()
        {
            new Thread(() =>
            {
                _ = LapTracker.Instance;
                _ = RaceSessionTracker.Instance;

                Debug.WriteLine("Started ACC.Trackers");
            }).Start();
        }
    }
}
