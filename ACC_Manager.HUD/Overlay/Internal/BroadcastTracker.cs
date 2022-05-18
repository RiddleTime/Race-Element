using ACC_Manager.Broadcast.Structs;
using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.Internal
{
    internal class BroadcastTracker : IDisposable
    {
        private static BroadcastTracker _instance;
        internal static BroadcastTracker Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BroadcastTracker();

                return _instance;
            }
        }

        private ACCUdpRemoteClient client;

        private BroadcastTracker()
        {
            // fetch from json....
            client = new ACCUdpRemoteClient("127.0.0.1", 9000, string.Empty, string.Empty, string.Empty, 100);

            client.MessageHandler.OnRealtimeUpdate += (s, realTimeUpdate) => OnRealTimeUpdate?.Invoke(this, realTimeUpdate);
            client.MessageHandler.OnConnectionStateChanged += (int connectionId, bool connectionSuccess, bool isReadonly, string error) => OnConnectionStateChanged?.Invoke(this, new ConnectionState()
            {
                ConnectionId = connectionId,
                ConnectionSuccess = connectionSuccess,
                IsReadonly = isReadonly,
                Error = error
            });
        }

        public event EventHandler<RealtimeUpdate> OnRealTimeUpdate;
        public event EventHandler<ConnectionState> OnConnectionStateChanged;

        public void Dispose()
        {
            this.client.Shutdown();
            this.client.Dispose();
        }
    }
}
