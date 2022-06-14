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
        public Streaming()
        {
            InitializeComponent();
            comboStreamSoftware.Items.Add("OBS");
            comboStreamSoftware.Items.Add("Streamlabs");
            comboStreamSoftware.SelectedIndex = 0;
            comboStreamSoftware.SelectionChanged += (s, e) =>
            {
                switch (comboStreamSoftware.SelectedItem)
                {
                    case "OBS":
                        {
                            apiTokenStack.Visibility = Visibility.Collapsed;
                            passwordStack.Visibility = Visibility.Visible;
                            break;
                        }
                    case "Streamlabs":
                        {
                            apiTokenStack.Visibility = Visibility.Visible;
                            passwordStack.Visibility = Visibility.Collapsed;
                            break;
                        }
                }
            };

            buttonSave.Click += (s, e) => SaveSettings();
            buttonTestConnnection.Click += (s, e) => TestConnection();

            toggleSetupHider.Click += (s, e) => ToggleSetupHider();

            this.IsVisibleChanged += Streaming_IsVisibleChanged;
        }

        private void Streaming_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
                LoadSettings();
        }

        private void TestConnection()
        {
            var streamSettings = StreamSettings.LoadJson();

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

        private void TestOBSconnection(StreamingSettingsJson streamingSettings)
        {
            Dispatcher.Invoke(() =>
            {
                buttonTestConnnection.IsEnabled = false;
            });
            Task.Run(() =>
            {
                try
                {
                    OBSWebsocket _obsWebSocket = new OBSWebsocket();
                    _obsWebSocket.Connected += (s, e) =>
                    {
                        MainWindow.Instance.ClearSnackbar();
                        MainWindow.Instance.EnqueueSnackbarMessage($"Connection to {streamingSettings.StreamingSoftware} is working.");
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
                            MainWindow.Instance.EnqueueSnackbarMessage($"Failed to make a connection to {streamingSettings.StreamingSoftware}.");
                        }

                        Dispatcher.Invoke(() =>
                        {
                            buttonTestConnnection.IsEnabled = true;
                        });

                    };
                    _obsWebSocket.Connect($"ws://{streamingSettings.StreamingWebSocketIP}:{streamingSettings.StreamingWebSocketPort}", streamingSettings.StreamingWebSocketPassword);
                }
                catch (Exception e)
                {
                    MainWindow.Instance.ClearSnackbar();
                    MainWindow.Instance.EnqueueSnackbarMessage($"Failed to make a connection to {streamingSettings.StreamingSoftware}.");
                    Dispatcher.Invoke(() =>
                    {
                        buttonTestConnnection.IsEnabled = true;
                    });
                }
            });
        }

        private void TestStreamlabsConnection(StreamingSettingsJson streamSettings)
        {
            Dispatcher.Invoke(() =>
            {
                buttonTestConnnection.IsEnabled = false;
            });
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
                            throw new Exception("Did not find the setup hider source.");
                        else
                        {
                            MainWindow.Instance.ClearSnackbar();
                            MainWindow.Instance.EnqueueSnackbarMessage($"Connection to {streamSettings.StreamingSoftware} is working.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);

                    string message = "Failed to make a connection.";

                    if (e.Message.Contains("setup hider"))
                        message = e.Message;

                    MainWindow.Instance.ClearSnackbar();
                    MainWindow.Instance.EnqueueSnackbarMessage(message);
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        buttonTestConnnection.IsEnabled = true;
                    });
                }
            });
        }


        private void ToggleSetupHider()
        {
            var streamingSettings = StreamSettings.LoadJson();
            streamingSettings.SetupHider = toggleSetupHider.IsChecked.Value;
            StreamSettings.SaveJson(streamingSettings);

            string status = streamingSettings.SetupHider ? "Enabled" : "Disabled";
            MainWindow.Instance.ClearSnackbar();
            MainWindow.Instance.EnqueueSnackbarMessage($"Stream Setup Hider {status}.");
        }

        private void LoadSettings()
        {
            var streamingSettings = StreamSettings.LoadJson();

            switch (streamingSettings.StreamingSoftware)
            {
                case "OBS":
                    {
                        streamPassword.Password = streamingSettings.StreamingWebSocketPassword;
                        break;
                    }
                case "Streamlabs":
                    {
                        apiToken.Password = streamingSettings.StreamingWebSocketPassword;
                        break;
                    }
            }

            streamServer.Text = streamingSettings.StreamingWebSocketIP;
            streamPort.Text = $"{streamingSettings.StreamingWebSocketPort}";
            comboStreamSoftware.SelectedItem = streamingSettings.StreamingSoftware;

            toggleSetupHider.IsChecked = streamingSettings.SetupHider;
        }

        private void SaveSettings()
        {
            var streamingSettings = StreamSettings.LoadJson();

            streamingSettings.StreamingSoftware = comboStreamSoftware.SelectedItem.ToString();
            streamingSettings.StreamingWebSocketIP = streamServer.Text;
            streamingSettings.StreamingWebSocketPort = int.Parse(streamPort.Text);

            switch (streamingSettings.StreamingSoftware)
            {
                case "OBS":
                    {
                        streamingSettings.StreamingWebSocketPassword = streamPassword.Password;
                        break;
                    }
                case "Streamlabs":
                    {
                        streamingSettings.StreamingWebSocketPassword = apiToken.Password;
                        break;
                    }
            }

            StreamSettings.SaveJson(streamingSettings);
            MainWindow.Instance.ClearSnackbar();
            MainWindow.Instance.EnqueueSnackbarMessage("Saved Streaming settings.");
        }
    }
}
