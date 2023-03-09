using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.Data.ACC.Tyres;
using System.Diagnostics;
using System.Threading;

namespace RaceElement.Data.ACC.Tracker
{
    public static class ACCTrackerStarter
    {
        public static void StartACCTrackers()
        {
            new Thread(() =>
            {
                _ = LapTracker.Instance;
                _ = RaceSessionTracker.Instance;
                _ = TyresTracker.Instance;

                Debug.WriteLine("Started ACC.Trackers");
            }).Start();
        }
    }
}
