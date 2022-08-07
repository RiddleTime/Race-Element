using ACCManager.Data.ACC.Session;
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
                _ = RaceSessionTracker.Instance;

                Debug.WriteLine("Started ACC.Trackers");
            }).Start();
        }
    }
}
