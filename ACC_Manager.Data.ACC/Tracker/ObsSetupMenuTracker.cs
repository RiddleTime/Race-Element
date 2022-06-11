using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Tracker
{
    internal class ObsSetupMenuTracker
    {
        public static ObsSetupMenuTracker _instance;
        public static ObsSetupMenuTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new ObsSetupMenuTracker();
                return _instance;
            }
        }

        private OBSWebsocket _obsWebSocket;

        private ObsSetupMenuTracker()
        {
            _obsWebSocket = new OBSWebsocket();
        }
    }
}
