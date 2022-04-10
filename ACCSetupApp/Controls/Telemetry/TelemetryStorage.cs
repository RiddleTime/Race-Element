using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls
{
    internal class TelemetryStorage
    {
        /// <summary>
        /// Timestamp (DateTime Ticks), (dataName, value)
        /// </summary>
        private Dictionary<long, Dictionary<string, object>> Storage;

        public TelemetryStorage()
        {
            Storage = new Dictionary<long, Dictionary<string, object>>();
        }

        public void AddDataEntry(long timestamp, Dictionary<string, object> data)
        {
            lock (Storage)
            {
                if (Storage.ContainsKey(timestamp))
                    return;
                Storage.Add(timestamp, data);
            }
        }

        public Dictionary<long, Dictionary<string, object>> GetAllData()
        {
            lock (Storage)
            {
                return Storage;
            }
        }
    }
}
