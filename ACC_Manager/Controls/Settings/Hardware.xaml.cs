using ACC_Manager.Util.Settings;
using ACCManager.Hardware.ACC.SteeringLock;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for Hardware.xaml
    /// </summary>
    public partial class Hardware : UserControl
    {
        public Hardware()
        {
            InitializeComponent();
            toggleSteeringHardwareLock.Checked += ToggleSteeringHardwareLock_Checked;
            toggleSteeringHardwareLock.Unchecked += ToggleSteeringHardwareLock_Unchecked;

            LoadSettings();
        }

        private void ToggleSteeringHardwareLock_Unchecked(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            SteeringLockTracker.Instance.Dispose();
            MainWindow.Instance.ClearSnackbar();
            MainWindow.Instance.EnqueueSnackbarMessage("Disabled automatic hardware steering lock.");

        }

        private void ToggleSteeringHardwareLock_Checked(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            SteeringLockTracker.Instance.StartTracking();
            MainWindow.Instance.ClearSnackbar();
            MainWindow.Instance.EnqueueSnackbarMessage("Enabled automatic hardware steering lock.");
        }

        private void LoadSettings()
        {
            try
            {
                var hardwareSettings = HardwareSettings.LoadJson();

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
                var hardwareSettings = HardwareSettings.LoadJson();

                hardwareSettings.UseHardwareSteeringLock = toggleSteeringHardwareLock.IsChecked.Value;

                HardwareSettings.SaveJson(hardwareSettings);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
