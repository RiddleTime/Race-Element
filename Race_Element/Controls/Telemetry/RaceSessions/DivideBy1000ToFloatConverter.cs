using System;
using System.Globalization;
using System.Windows.Data;

namespace RaceElement.Controls.Telemetry.RaceSessions
{
    internal class DivideBy1000ToFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                double result = (int)value / 1000d;
                return $"{result:F3}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
