using ACCManager.Data.ACC.EntryList;
using ACCManager.Data.ACC.Session;
using ACCManager.Data.ACC.Tracker.Laps;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Tracker
{
    public class DataTrackerDispose
    {
        public static void Dispose()
        {
            Debug.WriteLine("Safely disposing ACC Data Tracker instances");
            BroadcastTracker.Instance.Dispose();
            EntryListTracker.Instance.Stop();
            RaceSessionTracker.Instance.Stop();
            SetupHiderTracker.Instance.Dispose();
            LapTracker.Instance.Stop();
        }
    }
}
