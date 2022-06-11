using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Controls.Settings
{
    internal class AccManagerSettings
    {
        public static AccManagerSettings _instance;
        public static AccManagerSettings Instance
        {
            get
            {
                if (_instance == null) _instance = new AccManagerSettings();
                return _instance;
            }
        }

        public ACCManagerSettingsJson GetSettings()
        {
            return null;
        }

        public void SaveSettings(ACCManagerSettingsJson settings)
        {
            if (settings == null)
                throw new ArgumentNullException("Provided a Null reference parameter for 'settings'");
        }

        public class ACCManagerSettingsJson
        {
            public StreamingSettings StreamingSettings { get; set; }
        }

        public class StreamingSettings
        {
            public string StreamingSoftware { get; set; }
            public string StreamingWebSocketIP { get; set; }
            public int StreamingWebSocketPort { get; set; }
            public string StreamingWebSocketPassword { get; set; }
        }
    }
}
