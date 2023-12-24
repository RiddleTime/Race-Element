using RaceElement.Data.ACC.Config;
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

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for AccCameraSettings.xaml
    /// </summary>
    public partial class AccCameraSettings : UserControl
    {
        private readonly CameraSettingService camera = new();

        public AccCameraSettings()
        {
            InitializeComponent();
            this.Loaded += (s, e) => LoadSettings();

            buttonResetHelicam.Click += (s, e) =>
            {
                camera.ResetHelicamCamera();
                LoadSettings();
            };

            sliderHelicamDistance.ValueChanged += (s, e) => SaveSettings();
            sliderHelicamFOV.ValueChanged += (s, e) => SaveSettings();
            sliderHelicamTargetMoreCars.ValueChanged += (s, e) => SaveSettings();
            sliderHelicamInterpolationTime.ValueChanged += (s, e) => SaveSettings();
            sliderHelicamTargetMaxDistance.ValueChanged += (s, e) => SaveSettings();
        }

        private void LoadSettings()
        {
            var settings = camera.Settings().Get(false);

            sliderHelicamDistance.Value = settings.HelicamDistance;
            sliderHelicamFOV.Value = settings.HelicamFOV;
            sliderHelicamTargetMoreCars.Value = settings.HelicamTargetMoreCars;
            sliderHelicamInterpolationTime.Value = settings.HelicamTargetInterpTime;
            sliderHelicamTargetMaxDistance.Value = settings.HelicamTargetCarsMaxDist;
        }

        private void SaveSettings()
        {
            var settings = camera.Settings().Get(false);

            settings.HelicamDistance = (int)sliderHelicamDistance.Value;
            settings.HelicamFOV = (int)sliderHelicamFOV.Value;
            settings.HelicamTargetMoreCars = (int)sliderHelicamTargetMoreCars.Value;
            settings.HelicamTargetCarsMaxDist = (int)sliderHelicamTargetMaxDistance.Value;
            settings.HelicamTargetInterpTime = (int)sliderHelicamInterpolationTime.Value;

            camera.Settings().Save(settings);
        }
    }
}
