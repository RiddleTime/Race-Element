using ACCManager.Controls.Settings;
using System;
using System.Collections.Generic;
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

        }

        private void SaveSettings()
        {
            var globalSettings = AccManagerSettings.Instance.GetSettings();
            var streamingSettings = globalSettings.StreamingSettings;

            streamServer.Text = streamingSettings.StreamingWebSocketIP;
            streamPort.Text = $"{streamingSettings.StreamingWebSocketPort}";
            streamPassword.Password = streamingSettings.StreamingWebSocketPassword;
        }

    }
}
