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

namespace ACCManager.Data.ACC.Tracker
{
    public class BroadcastTracker : IDisposable
    {
        private static BroadcastTracker _instance;
        public static BroadcastTracker Instance
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
        }

        public event EventHandler<RealtimeUpdate> OnRealTimeUpdate;
        public event EventHandler<ConnectionState> OnConnectionStateChanged;
        public event EventHandler<TrackData> OnTrackDataUpdate;
        public event EventHandler<RealtimeCarUpdate> OnRealTimeCarUpdate;

        internal void Connect()
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

            client.MessageHandler.OnConnectionStateChanged += (connectionId, connectionSuccess, isReadonly, error) =>
            {
                Debug.WriteLine($"Broadcast Connection State Changed:\n{connectionId}, Connected: {connectionSuccess}, IsReadOnly: {isReadonly}\nError: {error}");
            };
        }

        private void ResetData()
        {
            OnConnectionStateChanged?.Invoke(this, new ConnectionState());
            OnRealTimeUpdate?.Invoke(this, new RealtimeUpdate());
            OnRealTimeCarUpdate?.Invoke(this, new RealtimeCarUpdate());
            OnTrackDataUpdate?.Invoke(this, new TrackData());
        }

        internal void Disconnect()
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
