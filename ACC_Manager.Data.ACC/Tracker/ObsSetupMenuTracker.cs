using ACCManager.Util;
using ACCManager.Util.Settings;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Tracker
{
    internal class ObsSetupMenuTracker : IDisposable
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

        private readonly OBSWebsocket _obsWebSocket;
        private bool _isTracking = false;
        private readonly ACCSharedMemory _sharedMemory;

        private ObsSetupMenuTracker()
        {
            if (StreamSettings.LoadJson().SetupHider)
            {
                Debug.WriteLine("Started OBS setup menu tracker");
                _obsWebSocket = new OBSWebsocket();
                _obsWebSocket.Connected += ObsWebSocket_Connected;

                _sharedMemory = new ACCSharedMemory();

                Connect();
                StartTracker();
            }
        }

        private void Connect()
        {
            try
            {
                var streamSettings = StreamSettings.LoadJson();

                _obsWebSocket.Connect($"ws://{streamSettings.StreamingWebSocketIP}:{streamSettings.StreamingWebSocketPort}", streamSettings.StreamingWebSocketPassword);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void StartTracker()
        {
            _isTracking = true;
            new Thread(() =>
            {
                while (_isTracking)
                {
                    Thread.Sleep(50);
                    var pageGraphics = _sharedMemory.ReadGraphicsPageFile();

                    Toggle(pageGraphics.IsSetupMenuVisible);
                }
            }).Start();
        }

        private void ObsWebSocket_Connected(object sender, EventArgs e)
        {
            Debug.WriteLine("Connected obs websocket");
            Debug.WriteLine($"Current scene: {_obsWebSocket.GetCurrentScene().Name}");
            Toggle(true);
        }

        public void Toggle(bool enable)
        {
            if (_obsWebSocket.IsConnected)
            {
                SourceInfo setupHiderSource = _obsWebSocket.GetSourcesList().Find(x => x.Name == "SetupHider");
                if (setupHiderSource != null)
                {
                    try
                    {
                        _obsWebSocket.SetSourceRender(setupHiderSource.Name, enable);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Setup Hider: Source 'SetupHider' was not found.");
                        LogWriter.WriteToLog($"Setup Hider: Source 'SetupHider' was not found.");
                        Dispose();
                    }
                }
                else
                {
                    Debug.WriteLine($"Setup Hider: Source 'SetupHider' was not found.");
                    LogWriter.WriteToLog($"Setup Hider: Source 'SetupHider' was not found.");
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            _isTracking = false;
            Debug.WriteLine("Disposed OBS setup menu tracker");
            if (StreamSettings.LoadJson().SetupHider)
                _obsWebSocket.Disconnect();

            _instance = null;
        }
    }
}
