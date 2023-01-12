using RaceElement.Util.Settings;
using RaceElement.Hardware.ACC.SteeringLock;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for Hardware.xaml
    /// </summary>
    public partial class Hardware : UserControl
    {
        private readonly HardwareSettings _hardwareSettings;
        public Hardware()
        {
            InitializeComponent();
            toggleSteeringHardwareLock.Checked += ToggleSteeringHardwareLock_Checked;
            toggleSteeringHardwareLock.Unchecked += ToggleSteeringHardwareLock_Unchecked;

            buttonCheckSteeringLockSupport.Click += ButtonCheckSteeringLockSupport_Click;

            _hardwareSettings = new HardwareSettings();
            this.Loaded += (s, e) => LoadSettings();
        }

        private void ButtonCheckSteeringLockSupport_Click(object sender, RoutedEventArgs e)
        {
            string supportedDeviceName = SteeringLockTracker.GetSupportedDeviceName();
            string message;

            if (supportedDeviceName == string.Empty)
            {
                message = "Your device is not supported.";
                toggleSteeringHardwareLock.IsChecked = false;
            }
            else
                message = $"Detect supported device: {supportedDeviceName}";

            MainWindow.Instance.ClearSnackbar();
            MainWindow.Instance.EnqueueSnackbarMessage(message);
        }

        private void ToggleSteeringHardwareLock_Unchecked(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            SteeringLockTracker.Instance.Dispose();
            TitleBar.Instance.SetIcons(TitleBar.ActivatedIcons.AutomaticSteeringHardLock, false);
            //MainWindow.Instance.EnqueueSnackbarMessage("Disabled automatic hardware steering lock.");

        }

        private void ToggleSteeringHardwareLock_Checked(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            SteeringLockTracker.Instance.StartTracking();
            TitleBar.Instance.SetIcons(TitleBar.ActivatedIcons.AutomaticSteeringHardLock, true);
            //MainWindow.Instance.EnqueueSnackbarMessage("Enabled automatic hardware steering lock.");
        }

        private void LoadSettings()
        {
            try
            {
                var hardwareSettings = _hardwareSettings.Get();

                toggleSteeringHardwareLock.IsChecked = hardwareSettings.UseHardwareSteeringLock;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void SaveSettings()
        {
            try
            {
                var hardwareSettings = _hardwareSettings.Get();

                hardwareSettings.UseHardwareSteeringLock = toggleSteeringHardwareLock.IsChecked.Value;

                _hardwareSettings.Save(hardwareSettings);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
