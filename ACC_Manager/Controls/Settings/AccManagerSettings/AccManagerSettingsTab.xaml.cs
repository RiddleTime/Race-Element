using ACC_Manager.Util.Settings;
using System;
using System.Threading;
using System.Windows.Controls;

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


            ThreadPool.QueueUserWorkItem(x =>
            {
                _settings = new AccManagerSettings();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    toggleRecordLapTelemetry.IsChecked = _settings.Get().TelemetryRecordDetailed;
                    sliderTelemetryHerz.Value = _settings.Get().TelemetryDetailedHerz;
                }));
            });

            toggleRecordLapTelemetry.Checked += (s, e) => SaveSettings();
            toggleRecordLapTelemetry.Unchecked += (s, e) => SaveSettings();

            sliderTelemetryHerz.ValueChanged += (s, e) => SaveSettings();

        }

        private void SaveSettings()
        {
            var settings = _settings.Get();
            settings.TelemetryRecordDetailed = toggleRecordLapTelemetry.IsChecked.Value;
            settings.TelemetryDetailedHerz = (int)sliderTelemetryHerz.Value;
            _settings.Save(settings);
        }
    }
}
