using ACC_Manager.Util.Settings;
using ACCManager.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using static ACCManager.Controls.LiveryBrowser;

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
            InitializeComponent();

            _settings = new AccManagerSettings();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                toggleRecordLapTelemetry.IsChecked = _settings.Get().TelemetryRecordDetailed;
                sliderTelemetryHerz.Value = _settings.Get().TelemetryDetailedHerz;
                toggleMinimizeToSystemTray.IsChecked = _settings.Get().MinimizeToSystemTray;

                toggleRecordLapTelemetry.Checked += (s, e) => SaveSettings();
                toggleRecordLapTelemetry.Unchecked += (s, e) => SaveSettings();

                toggleMinimizeToSystemTray.Checked += (s, e) => SaveSettings();
                toggleMinimizeToSystemTray.Unchecked += (s, e) => SaveSettings();

                sliderTelemetryHerz.ValueChanged += (s, e) => SaveSettings();

                buttonOpenAccManagerFolder.Click += ButtonOpenAccManagerFolder_Click;
            }));
        }

        private void ButtonOpenAccManagerFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo($"{FileUtil.AccManagerAppDataPath}");
            Process.Start(directory.FullName);
        }

        private void SaveSettings()
        {
            var settings = _settings.Get();

            settings.MinimizeToSystemTray = toggleMinimizeToSystemTray.IsChecked.Value;

            settings.TelemetryRecordDetailed = toggleRecordLapTelemetry.IsChecked.Value;
            settings.TelemetryDetailedHerz = (int)sliderTelemetryHerz.Value;
            _settings.Save(settings);
        }
    }
}
