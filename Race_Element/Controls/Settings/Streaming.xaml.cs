using RaceElement.Data.ACC.Tracker;
using RaceElement.Util.Settings;
using OBSWebsocketDotNet;
using SLOBSharp.Client;
using SLOBSharp.Client.Requests;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WebSocketSharp;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for Streaming.xaml
    /// </summary>
    public partial class Streaming : UserControl
    {
        private StreamSettings _streamSettings;

        public Streaming()
        {
            InitializeComponent();

            ThreadPool.QueueUserWorkItem(x =>
            {
                Thread.Sleep(1000);
                _streamSettings = new StreamSettings();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TitleBar.Instance.SetIcons(TitleBar.ActivatedIcons.SetupHider, _streamSettings.Get().SetupHider);
                }));
            });

            comboStreamSoftware.Items.Add("OBS");
            comboStreamSoftware.Items.Add("Streamlabs");
            comboStreamSoftware.SelectedIndex = 0;
            comboStreamSoftware.SelectionChanged += (s, e) =>
            {
                switch (comboStreamSoftware.SelectedItem)
                {
                    case "OBS":
                        {
                            obsStack.Visibility = Visibility.Visible;
                            break;
                        }
                    case "Streamlabs":
                        {
                            obsStack.Visibility = Visibility.Hidden;
                            break;
                        }
                }
            };

            buttonSave.Click += (s, e) => SaveSettings();
            buttonTestConnnection.Click += (s, e) => TestConnection();

            toggleSetupHider.Click += (s, e) => ToggleSetupHider();

            this.IsVisibleChanged += Streaming_IsVisibleChanged;
            var pageGraphics = PageGraphicsTracker.Instance;
        }

        private void Streaming_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
                LoadSettings();
        }

        private void TestConnection()
        {
            var streamSettings = _streamSettings.Get();
            buttonTestConnnection.Content = "Testing Connection...";
            buttonTestConnnection.IsEnabled = false;

            switch (comboStreamSoftware.SelectedItem)
            {
                case "OBS":
                    {
                        TestOBSconnection(streamSettings);
                        break;
                    }
                case "Streamlabs":
                    {
                        TestStreamlabsConnection(streamSettings);
                        break;
                    }
            }
        }

        private void TestOBSconnection(StreamingSettingsJson streamSettings)
        {
            Task.Run(() =>
            {
                try
                {
                    OBSWebsocket _obsWebSocket = new();
                    _obsWebSocket.Connected += (s, e) =>
                    {
                        Task.Run(() =>
                        {
                            var list = _obsWebSocket.GetSceneItemList(_obsWebSocket.GetCurrentProgramScene());

                            bool foundSetupHider = list.Find(x => x.SourceName == "SetupHider") != null;

                            string message = "SetupHider source was not found in your current Scene.";
                            if (foundSetupHider)
                                message = $"Connection to {streamSettings.StreamingSoftware} is working.";

                            MainWindow.Instance.ClearSnackbar();
                            MainWindow.Instance.EnqueueSnackbarMessage(message);

                            _obsWebSocket.Disconnect();
                        });
                    };
                    _obsWebSocket.Disconnected += (s, e) =>
                    {
                        if (e.WebsocketDisconnectionInfo.Type == Websocket.Client.DisconnectionType.Error)
                        {
                            Debug.WriteLine("Disconnected test connection.");
                            MainWindow.Instance.ClearSnackbar();
                            MainWindow.Instance.EnqueueSnackbarMessage($"Failed to make a connection to {streamSettings.StreamingSoftware}.");
                        }

                        Dispatcher.Invoke(() =>
                        {
                            buttonTestConnnection.Content = "Test Connection";
                            buttonTestConnnection.IsEnabled = true;
                        });
                    };
                    _obsWebSocket.ConnectAsync($"ws://{streamSettings.StreamingWebSocketIP}:{streamSettings.StreamingWebSocketPort}", streamSettings.StreamingWebSocketPassword);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    MainWindow.Instance.ClearSnackbar();
                    MainWindow.Instance.EnqueueSnackbarMessage($"Failed to make a connection to OBS.");
                    Dispatcher.Invoke(() =>
                    {
                        buttonTestConnnection.Content = "Test Connection";
                        buttonTestConnnection.IsEnabled = true;
                    });
                }
            });
        }

        private void TestStreamlabsConnection(StreamingSettingsJson streamSettings)
        {
            Task.Run(() =>
            {
                try
                {
                    SlobsPipeClient client = new();
                    var request = SlobsRequestBuilder.NewRequest().SetMethod("getScenes").SetResource("ScenesService").BuildRequest();

                    var response = client.ExecuteRequest(request);
                    if (response.Error != null)
                        throw new Exception($"{response.Error.Code}: {response.Error.Message} ");
                    else
                    {
                        bool foundSetupHider = false;
                        response.Result.First().Nodes.ForEach(x => { if (x.Name == "SetupHider") foundSetupHider = true; });
                        if (!foundSetupHider)
                            throw new Exception("Did not find the 'SetupHider' Source in your active Scene.");
                        else
                        {
                            MainWindow.Instance.ClearSnackbar();
                            MainWindow.Instance.EnqueueSnackbarMessage($"Connection to StreamLabs is working.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);

                    string message = $"Failed to make a connection to StreamLabs.";

                    if (e.Message.Contains("SetupHider"))
                        message = e.Message;

                    MainWindow.Instance.ClearSnackbar();
                    MainWindow.Instance.EnqueueSnackbarMessage(message);
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        buttonTestConnnection.Content = "Test Connection";
                        buttonTestConnnection.IsEnabled = true;
                    });
                }
            });
        }


        private void ToggleSetupHider()
        {
            var streamingSettings = _streamSettings.Get();
            streamingSettings.SetupHider = toggleSetupHider.IsChecked.Value;
            _streamSettings.Save(streamingSettings);

            TitleBar.Instance.SetIcons(TitleBar.ActivatedIcons.SetupHider, streamingSettings.SetupHider);
        }

        private void LoadSettings()
        {
            var streamingSettings = _streamSettings.Get();

            streamPassword.Password = streamingSettings.StreamingWebSocketPassword;
            streamServer.Text = streamingSettings.StreamingWebSocketIP;
            streamPort.Text = $"{streamingSettings.StreamingWebSocketPort}";
            comboStreamSoftware.SelectedItem = streamingSettings.StreamingSoftware;

            toggleSetupHider.IsChecked = streamingSettings.SetupHider;

            if (streamingSettings.SetupHider) TitleBar.Instance.SetIcons(TitleBar.ActivatedIcons.SetupHider, true);
        }

        private void SaveSettings()
        {
            var streamingSettings = _streamSettings.Get();

            streamingSettings.StreamingSoftware = comboStreamSoftware.SelectedItem.ToString();
            streamingSettings.StreamingWebSocketIP = streamServer.Text;
            streamingSettings.StreamingWebSocketPort = streamPort.Text;

            switch (streamingSettings.StreamingSoftware)
            {
                case "OBS":
                    {
                        streamingSettings.StreamingWebSocketPassword = streamPassword.Password;
                        break;
                    }
            }

            _streamSettings.Save(streamingSettings);
            MainWindow.Instance.ClearSnackbar();
            MainWindow.Instance.EnqueueSnackbarMessage("Saved Streaming settings.");
        }
    }
}
