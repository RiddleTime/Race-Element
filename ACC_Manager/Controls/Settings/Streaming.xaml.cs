using ACCManager.Data.ACC.Tracker;
using ACCManager.Util;
using ACCManager.Util.Settings;
using Newtonsoft.Json;
using OBSWebsocketDotNet;
using SLOBSharp.Client;
using SLOBSharp.Client.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebSocketSharp;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for Streaming.xaml
    /// </summary>
    public partial class Streaming : UserControl
    {
        private readonly StreamSettings _streamSettings;

        public Streaming()
        {
            InitializeComponent();

            _streamSettings = new StreamSettings();

            TitleBar.Instance.SetIcons(TitleBar.ActivatedIcons.SetupHider, _streamSettings.Get().SetupHider);

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
                    OBSWebsocket _obsWebSocket = new OBSWebsocket();
                    _obsWebSocket.Connected += (s, e) =>
                    {
                        bool foundSetupHider = _obsWebSocket.GetSourcesList().Find(x => x.Name == "SetupHider") != null;

                        string message = "SetupHider source was not found in your current Scene.";
                        if (foundSetupHider)
                            message = $"Connection to {streamSettings.StreamingSoftware} is working.";

                        MainWindow.Instance.ClearSnackbar();
                        MainWindow.Instance.EnqueueSnackbarMessage(message);

                        _obsWebSocket.Disconnect();
                    };
                    _obsWebSocket.Disconnected += (s, e) =>
                    {
                        CloseEventArgs args = (CloseEventArgs)e;
                        if (args.WasClean)
                            Debug.WriteLine("Disconnected test connection.");
                        else
                        {
                            MainWindow.Instance.ClearSnackbar();
                            MainWindow.Instance.EnqueueSnackbarMessage($"Failed to make a connection to {streamSettings.StreamingSoftware}.");
                        }

                        Dispatcher.Invoke(() =>
                        {
                            buttonTestConnnection.Content = "Test Connection";
                            buttonTestConnnection.IsEnabled = true;
                        });

                    };
                    _obsWebSocket.Connect($"ws://{streamSettings.StreamingWebSocketIP}:{streamSettings.StreamingWebSocketPort}", streamSettings.StreamingWebSocketPassword);
                }
                catch (Exception)
                {
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
                    SlobsPipeClient client = new SlobsPipeClient();
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
