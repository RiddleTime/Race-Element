using RaceElement.Util.Settings;
using RaceElement.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using static RaceElement.Controls.LiveryBrowser;

namespace RaceElement.Controls
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
                LoadSettings();

                toggleGenerate4kDDS.Checked += (s, e) => SaveSettings();
                toggleGenerate4kDDS.Unchecked += (s, e) => SaveSettings();

                toggleRecordLapTelemetry.Checked += (s, e) => SaveSettings();
                toggleRecordLapTelemetry.Unchecked += (s, e) => SaveSettings();

                toggleMinimizeToSystemTray.Checked += (s, e) => SaveSettings();
                toggleMinimizeToSystemTray.Unchecked += (s, e) => SaveSettings();

                sliderTelemetryHerz.ValueChanged += (s, e) => SaveSettings();

                buttonOpenAccManagerFolder.Click += ButtonOpenAccManagerFolder_Click;
            }));
        }

        private void LoadSettings()
        {
            toggleRecordLapTelemetry.IsChecked = _settings.Get().TelemetryRecordDetailed;
            sliderTelemetryHerz.Value = _settings.Get().TelemetryDetailedHerz;
            labelTelemetryHerz.Content = $"Telemetry: Extended Data Herz: {_settings.Get().TelemetryDetailedHerz}";
            toggleMinimizeToSystemTray.IsChecked = _settings.Get().MinimizeToSystemTray;
            toggleGenerate4kDDS.IsChecked = _settings.Get().Generate4kDDS;
        }

        private void ButtonOpenAccManagerFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DirectoryInfo directory = new($"{FileUtil.RaceElementAppDataPath}");
            Process.Start("explorer", directory.FullName);
        }

        private void SaveSettings()
        {
            var settings = _settings.Get();

            settings.MinimizeToSystemTray = toggleMinimizeToSystemTray.IsChecked.Value;

            settings.TelemetryRecordDetailed = toggleRecordLapTelemetry.IsChecked.Value;
            settings.TelemetryDetailedHerz = (int)sliderTelemetryHerz.Value;
            settings.Generate4kDDS = toggleGenerate4kDDS.IsChecked.Value;

            labelTelemetryHerz.Content = $"Telemetry: Extended Data Herz: {settings.TelemetryDetailedHerz}";

            _settings.Save(settings);
        }
    }
}
