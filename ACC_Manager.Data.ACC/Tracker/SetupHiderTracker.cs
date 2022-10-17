using ACCManager.Util.Settings;
using OBSWebsocketDotNet;
using SLOBSharp.Client;
using SLOBSharp.Client.Requests;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

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

        private bool _toggle = true;

        private readonly StreamSettings _streamSettings;

        private SetupHiderTracker()
        {
            _streamSettings = new StreamSettings();
        }

        public void StartTracker()
        {

            var streamSettings = _streamSettings.Get();
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

                IsTracking = true;
                new Thread(() =>
                {
                    while (IsTracking)
                    {
                        Thread.Sleep(50);
                        var pageGraphics = ACCSharedMemory.Instance.ReadGraphicsPageFile(true);

                        if (pageGraphics.Status != ACCSharedMemory.AcStatus.AC_OFF)
                            if (pageGraphics.IsSetupMenuVisible != _toggle)
                            {
                                _toggle = pageGraphics.IsSetupMenuVisible;
                                Toggle(pageGraphics.IsSetupMenuVisible);
                            }
                    }
                }).Start();
            }
        }


        private void Connect()
        {
            try
            {
                var streamSettings = _streamSettings.Get();

                if (streamSettings.StreamingSoftware == "OBS")
                    _obsWebSocket.ConnectAsync($"ws://{streamSettings.StreamingWebSocketIP}:{streamSettings.StreamingWebSocketPort}", streamSettings.StreamingWebSocketPassword);
            }
            catch (Exception)
            {
                //Debug.WriteLine(ex.ToString());
            }
        }

        private void ObsWebSocket_Connected(object sender, EventArgs e)
        {
        }

        public void Toggle(bool enable)
        {
            Trace.WriteLine("Setup hider is " + (enable ? "Visible" : "Hidden"));
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
                    string currentScene = _obsWebSocket.GetCurrentProgramScene();
                    var list = _obsWebSocket.GetSceneItemList(currentScene);

                    var sceneItemSetupHider = list.Find(x => x.SourceName == "SetupHider");
                    if (sceneItemSetupHider != null)
                    {
                        _obsWebSocket.SetSceneItemEnabled(currentScene, sceneItemSetupHider.ItemId, enable);
                    }
                }
            }
            catch (Exception)
            {
                //Debug.WriteLine(e);
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
                    var node = response.Result.First().Nodes.Find(x => x.Name == "SetupHider");

                    if (node != null)
                    {
                        if (node.Visible != enable)
                        {
                            request = SlobsRequestBuilder.NewRequest().SetMethod("setVisibility").SetResource(node.ResourceId).AddArgs(enable).BuildRequest();
                            _SlobsClient.ExecuteRequest(request);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Debug.WriteLine(e);
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
