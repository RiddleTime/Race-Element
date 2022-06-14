using ACCManager.Util;
using ACCManager.Util.Settings;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using SLOBSharp.Client;
using SLOBSharp.Client.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Tracker
{
    internal class SetupHiderTracker : IDisposable
    {
        public static SetupHiderTracker _instance;
        public static SetupHiderTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new SetupHiderTracker();
                return _instance;
            }
        }

        private readonly OBSWebsocket _obsWebSocket;
        private readonly SlobsPipeClient _SlobsClient;
        private bool _isTracking = false;
        private readonly ACCSharedMemory _sharedMemory;

        private bool _toggle = false;

        private SetupHiderTracker()
        {
            var streamSettings = StreamSettings.LoadJson();
            if (streamSettings.SetupHider)
            {
                Debug.WriteLine("Started Setup Hider Tracker");

                if (streamSettings.StreamingSoftware == "OBS")
                {
                    _obsWebSocket = new OBSWebsocket();
                    _obsWebSocket.Connected += ObsWebSocket_Connected;
                }
                else
                {
                    _SlobsClient = new SlobsPipeClient();
                }

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

                if (streamSettings.StreamingSoftware == "OBS")
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

                    if (pageGraphics.IsSetupMenuVisible != _toggle)
                    {
                        Toggle(pageGraphics.IsSetupMenuVisible);
                        _toggle = pageGraphics.IsSetupMenuVisible;
                    }
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
            if (_obsWebSocket != null)
                ToggleOBS(enable);
            else
                ToggleStreamLabs(enable);
        }

        private void ToggleOBS(bool enable)
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

        private void ToggleStreamLabs(bool enable)
        {
            try
            {
                var request = SlobsRequestBuilder.NewRequest().SetMethod("getScenes").SetResource("ScenesService").BuildRequest();

                var response = _SlobsClient.ExecuteRequest(request);
                if (response.Error != null)
                    throw new Exception($"{response.Error.Code}: {response.Error.Message} ");
                else
                {
                    string resource = string.Empty;
                    response.Result.First().Nodes.ForEach(x => { if (x.Name == "SetupHider") resource = x.ResourceId; });
                    if (resource != string.Empty)
                    {
                        request = SlobsRequestBuilder.NewRequest().SetMethod("setVisibility").SetResource(resource).AddArgs(enable).BuildRequest();
                        _SlobsClient.ExecuteRequest(request);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
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
