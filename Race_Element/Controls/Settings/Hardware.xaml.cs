using RaceElement.Util.Settings;
using RaceElement.Hardware.ACC.SteeringLock;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using RaceElement.Data.ACC.Cars;
using static RaceElement.Data.ConversionFactory;
using SharpCompress;
using RaceElement.Data;
using System.Linq;
using System.Windows.Media;

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
            this.Loaded += (s, e) =>
            {
                PopulateSteeringLocks();
                LoadSettings();
            };
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

        private void PopulateSteeringLocks()
        {
            stackerAllLocks.Children.Clear();

            CarModels[] carModels = (CarModels[])Enum.GetValues(typeof(CarModels));
            carModels.ForEach(carModel =>
            {
                var search = ConversionFactory.ParseNames.Where(x => x.Value == carModel);
                if (search.Any())
                {
                    int lockToLock = SteeringLock.Get(search.First().Key);
                    string carName = ConversionFactory.GetNameFromCarModel(carModel);

                    Label label = new()
                    {
                        Content = $"  {lockToLock}   |   {carName}",
                        FontWeight = FontWeights.Bold,
                        FontStyle = FontStyles.Italic,
                        FontSize = 15,
                        BorderBrush = new SolidColorBrush(Color.FromArgb(90,0,0,0)),
                        BorderThickness = new Thickness(0,0,0,2),
                    };
                    stackerAllLocks.Children.Add(label);
                }


            });
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
