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
    public class SetupHiderTracker : IDisposable
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

        public bool IsTracking = false;

        private OBSWebsocket _obsWebSocket;
        private SlobsPipeClient _SlobsClient;
        private ACCSharedMemory _sharedMemory;

        private bool _toggle = false;

        private SetupHiderTracker() { }

        public void StartTracker()
        {
            var streamSettings = StreamSettings.LoadJson();
            if (streamSettings.SetupHider)
            {
                if (streamSettings.StreamingSoftware == "OBS")
                {
                    _obsWebSocket = new OBSWebsocket();
                    _obsWebSocket.Connected += ObsWebSocket_Connected;
                    Connect();
                }
                else
                {
                    _SlobsClient = new SlobsPipeClient();
                }

                _sharedMemory = new ACCSharedMemory();
            }

            IsTracking = true;
            new Thread(() =>
            {
                while (IsTracking)
                {
                    Thread.Sleep(50);
                    var pageGraphics = _sharedMemory.ReadGraphicsPageFile();

                    if (pageGraphics.IsSetupMenuVisible != _toggle)
                    {
                        _toggle = pageGraphics.IsSetupMenuVisible;
                        Toggle(pageGraphics.IsSetupMenuVisible);
                    }
                }
            }).Start();
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

        private void ObsWebSocket_Connected(object sender, EventArgs e)
        {
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
            try
            {
                if (_obsWebSocket.IsConnected)
                {
                    SourceInfo setupHiderSource = _obsWebSocket.GetSourcesList().Find(x => x.Name == "SetupHider");
                    if (setupHiderSource != null)
                        _obsWebSocket.SetSourceRender(setupHiderSource.Name, enable);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
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
            IsTracking = false;
            if (_obsWebSocket != null)
                _obsWebSocket.Disconnect();

            _instance = null;
        }
    }
}
