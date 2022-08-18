using ACC_Manager.Util.Settings;
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
    /// Interaction logic for AccManagerSettingsTab.xaml
    /// </summary>
    public partial class AccManagerSettingsTab : UserControl
    {
        AccManagerSettings _settings;

        public AccManagerSettingsTab()
        {
            _settings = new AccManagerSettings();

            InitializeComponent();

            toggleRecordLapTelemetry.IsChecked = _settings.Get().TelemetryRecordDetailed;

            toggleRecordLapTelemetry.Checked += (s, e) => SaveTelemetryRecordDetailed();
            toggleRecordLapTelemetry.Unchecked += (s, e) => SaveTelemetryRecordDetailed();

        }

        private void SaveTelemetryRecordDetailed()
        {
            var settings = _settings.Get();
            settings.TelemetryRecordDetailed = toggleRecordLapTelemetry.IsChecked.Value;
            _settings.Save(settings);
        }
    }
}
