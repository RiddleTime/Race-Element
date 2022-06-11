using ACCManager.Util;
using ACCManager.Util.Settings;
using Newtonsoft.Json;
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

            LoadSettings();
        }

        private void LoadSettings()
        {
            var streamingSettings = StreamSettings.LoadJson();
            if (streamingSettings == null)
                streamingSettings = StreamingSettingsJson.Default();

            comboStreamSoftware.SelectedItem = streamingSettings.StreamingSoftware;
            streamServer.Text = streamingSettings.StreamingWebSocketIP;
            streamPort.Text = $"{streamingSettings.StreamingWebSocketPort}";
            streamPassword.Password = streamingSettings.StreamingWebSocketPassword;
        }

        private void SaveSettings()
        {
            var streamingSettings = StreamSettings.LoadJson();

            streamingSettings.StreamingSoftware = comboStreamSoftware.SelectedItem.ToString();
            streamingSettings.StreamingWebSocketIP = streamServer.Text;
            streamingSettings.StreamingWebSocketPort = int.Parse(streamPort.Text);
            streamingSettings.StreamingWebSocketPassword = streamPassword.Password;

            StreamSettings.SaveJson(streamingSettings);
            MainWindow.Instance.EnqueueSnackbarMessage("Saved Streaming settings.");
        }
    }
}
