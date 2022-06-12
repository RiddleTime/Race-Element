using ACCManager.Util;
using ACCManager.Util.Settings;
using Newtonsoft.Json;
using OBSWebsocketDotNet;
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

            buttonSave.Click += (s, e) => SaveSettings();
            buttonTestConnnection.Click += (s, e) => TestConnection();

            toggleSetupHider.Click += (s, e) => ToggleSetupHider();

            LoadSettings();
        }

        private void TestConnection()
        {
            var streamSettings = StreamSettings.LoadJson();

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
                        MainWindow.Instance.EnqueueSnackbarMessage($"Connection to {streamSettings.StreamingSoftware} is working.");
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
                            buttonTestConnnection.IsEnabled = true;
                        });

                    };
                    _obsWebSocket.Connect($"ws://{streamSettings.StreamingWebSocketIP}:{streamSettings.StreamingWebSocketPort}", streamSettings.StreamingWebSocketPassword);
                }
                catch (Exception e)
                {
                    MainWindow.Instance.ClearSnackbar();
                    MainWindow.Instance.EnqueueSnackbarMessage("Failed to make a connection");
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

            comboStreamSoftware.SelectedItem = streamingSettings.StreamingSoftware;
            streamServer.Text = streamingSettings.StreamingWebSocketIP;
            streamPort.Text = $"{streamingSettings.StreamingWebSocketPort}";
            streamPassword.Password = streamingSettings.StreamingWebSocketPassword;
            toggleSetupHider.IsChecked = streamingSettings.SetupHider;
        }

        private void SaveSettings()
        {
            var streamingSettings = StreamSettings.LoadJson();

            streamingSettings.StreamingSoftware = comboStreamSoftware.SelectedItem.ToString();
            streamingSettings.StreamingWebSocketIP = streamServer.Text;
            streamingSettings.StreamingWebSocketPort = int.Parse(streamPort.Text);
            streamingSettings.StreamingWebSocketPassword = streamPassword.Password;

            StreamSettings.SaveJson(streamingSettings);
            MainWindow.Instance.ClearSnackbar();
            MainWindow.Instance.EnqueueSnackbarMessage("Saved Streaming settings.");
        }
    }
}
