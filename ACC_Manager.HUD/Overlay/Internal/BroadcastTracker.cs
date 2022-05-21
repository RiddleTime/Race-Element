using ACC_Manager.Broadcast;
using ACC_Manager.Broadcast.Structs;
using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private ACCSharedMemory sharedMemory;
        public bool IsConnected { get; private set; }

        private BroadcastTracker()
        {

            sharedMemory = new ACCSharedMemory();
            // fetch from json....

            //this.Connect();


        }

        public event EventHandler<RealtimeUpdate> OnRealTimeUpdate;
        public event EventHandler<ConnectionState> OnConnectionStateChanged;
        public event EventHandler<TrackData> OnTrackDataUpdate;
        public event EventHandler<RealtimeCarUpdate> OnRealTimeCarUpdate;

        public void Connect()
        {
            this.IsConnected = true;

            BroadcastConfig.Root config = BroadcastConfig.GetConfiguration();
            client = new ACCUdpRemoteClient("127.0.0.1", config.updListenerPort, string.Empty, config.connectionPassword, config.commandPassword, 200);
            client.MessageHandler.OnRealtimeUpdate += (s, realTimeUpdate) =>
            {
                OnRealTimeUpdate?.Invoke(this, realTimeUpdate);
            };
            client.MessageHandler.OnConnectionStateChanged += (int connectionId, bool connectionSuccess, bool isReadonly, string error) =>
            {
                ConnectionState state = new ConnectionState()
                {
                    ConnectionId = connectionId,
                    ConnectionSuccess = connectionSuccess,
                    IsReadonly = isReadonly,
                    Error = error
                };

                OnConnectionStateChanged?.Invoke(this, state);
            };
            client.MessageHandler.OnTrackDataUpdate += (s, trackData) => OnTrackDataUpdate?.Invoke(this, trackData);

            client.MessageHandler.OnRealtimeCarUpdate += (s, e) =>
            {
                int localCarIndex = sharedMemory.ReadGraphicsPageFile().PlayerCarID;

                if (e.CarIndex == localCarIndex)
                    OnRealTimeCarUpdate?.Invoke(this, e);
            };
        }

        private void ResetData()
        {
            OnConnectionStateChanged?.Invoke(this, new ConnectionState());
            OnRealTimeUpdate?.Invoke(this, new RealtimeUpdate());
            OnRealTimeCarUpdate?.Invoke(this, new RealtimeCarUpdate());
            OnTrackDataUpdate?.Invoke(this, new TrackData());
        }

        public void Disconnect()
        {
            this.IsConnected = false;
            ResetData();

            if (client != null)
            {
                client.Shutdown();
                client.Dispose();
            }
        }

        public void Dispose()
        {
            this.Disconnect();
        }
    }
}
