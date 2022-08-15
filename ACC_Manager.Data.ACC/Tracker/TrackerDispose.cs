using ACCManager.Data.ACC.Database;
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
    public class ACCTrackerDispose
    {
        public static void Dispose()
        {
            Debug.WriteLine("Safely disposing ACC Data Tracker instances");
            BroadcastTracker.Instance.Dispose();
            EntryListTracker.Instance.Stop();
            
            SetupHiderTracker.Instance.Dispose();


            RaceSessionTracker.Instance.Stop();
            LapTracker.Instance.Stop();

            RaceWeekendDatabase.Close();
        }
    }
}
