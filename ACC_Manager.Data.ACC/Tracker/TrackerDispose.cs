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
        }
    }
}
