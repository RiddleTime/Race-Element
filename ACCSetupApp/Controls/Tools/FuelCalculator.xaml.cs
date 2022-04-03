using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for FuelCalculator.xaml
    /// </summary>
    public partial class FuelCalculator : UserControl
    {
        private int raceHours = 0;
        private int raceMinutes = 0;

        private double fuelPerLap = 0;

        private int lapTimeMinutes = 0;
        private int lapTimeSeconds = 0;
        private int lapTimeMilliseconds = 0;


        public FuelCalculator()
        {
            InitializeComponent();

            sliderHours.ValueChanged += SliderHours_ValueChanged;
            sliderMinutes.ValueChanged += SliderMinutes_ValueChanged;

            // validate input 
            textBoxFuelPerLap.PreviewTextInput += TextBoxFuelPerLap_PreviewTextInput;
            textBoxLapTimeMinute.PreviewTextInput += PreviewTextInput_NumbersOnly;
            textBoxLapTimeSecond.PreviewTextInput += PreviewTextInput_NumbersOnly;
            textBoxLapTimeMillis.PreviewTextInput += PreviewTextInput_NumbersOnly;

            textBoxLapTimeMinute.TextChanged += TextBoxLapTimeMinute_TextChanged;
            textBoxLapTimeSecond.TextChanged += TextBoxLapTimeSecond_TextChanged;
            textBoxLapTimeMillis.TextChanged += TextBoxLapTimeMillis_TextChanged;

            textBoxFuelPerLap.TextChanged += TextBoxFuelPerLap_TextChanged;

            UpdateRaceDuration();
            UpdateLapTime();
        }

        private void CalculateFuel()
        {
            long totalRaceMilliseconds = raceHours * 60 * 60 * 1000 + raceMinutes * 60 * 1000;
            long totalLapTimeMilliseconds = lapTimeMinutes * 60 * 1000 + lapTimeSeconds * 1000 + lapTimeMilliseconds;

            if (totalRaceMilliseconds == 0 || totalLapTimeMilliseconds == 0 || fuelPerLap == 0)
            {
                lapCountLabel.Content = string.Empty;
                fuelRequiredLabel.Content = string.Empty;
                return;
            }

            double laps = (double)totalRaceMilliseconds / (double)totalLapTimeMilliseconds;
            double fuelRequired = Math.Ceiling(laps) * fuelPerLap;

            lapCountLabel.Content = $"Laps: {Math.Ceiling(laps)} ({Math.Round(laps, 3)})";
            fuelRequiredLabel.Content = $"Fuel Required: {Math.Ceiling(fuelRequired)} Liters ({Math.Round(fuelRequired, 3)})";
        }

        private void TextBoxFuelPerLap_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = textBoxFuelPerLap.Text;
            if (text.Length > 0)
            {
                try
                {
                    fuelPerLap = Convert.ToDouble(text);
                    CalculateFuel();
                }
                catch (Exception)
                {
                    fuelPerLap = 0.0;
                }
            }
            else
            {
                fuelPerLap = 0.0;
            }
        }


        private void TextBoxLapTimeMillis_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = textBoxLapTimeMillis.Text;
            if (text.Length > 0)
                lapTimeMilliseconds = Convert.ToInt32(text);
            else
                lapTimeMilliseconds = 0;

            UpdateLapTime();
        }

        private void TextBoxLapTimeSecond_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = textBoxLapTimeSecond.Text;
            if (text.Length > 0)
                lapTimeSeconds = Convert.ToInt32(text);
            else
                lapTimeSeconds = 0;

            UpdateLapTime();
        }

        private void TextBoxLapTimeMinute_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = textBoxLapTimeMinute.Text;
            if (text.Length > 0)
                lapTimeMinutes = Convert.ToInt32(text);
            else
                lapTimeMinutes = 0;

            UpdateLapTime();
        }

        private void UpdateLapTime()
        {
            lapTimeLabel.Content = $"Laptime: {lapTimeMinutes}:{lapTimeSeconds}.{lapTimeMilliseconds}";
            CalculateFuel();
        }

        private void PreviewTextInput_NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxFuelPerLap_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void SliderMinutes_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateRaceDuration();
        }

        private void SliderHours_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateRaceDuration();
        }

        private void UpdateRaceDuration()
        {
            raceHours = (int)sliderHours.Value;
            raceMinutes = (int)sliderMinutes.Value;

            bool includeMinutes = raceMinutes > 0;

            string duration = string.Empty;
            if (raceHours > 0)
            {
                string hourOrHours = "hour";
                if (raceHours > 1)
                    hourOrHours += 's';

                duration += $"{raceHours} {hourOrHours}";

                if (includeMinutes)
                {
                    duration += " and ";
                }
            }

            if (includeMinutes)
            {
                duration += $"{raceMinutes} minutes";
            }

            raceDurationLabel.Content = $"Race Duration: {duration}";
            CalculateFuel();
        }


    }
}
