using System;
using System.Globalization;
using System.Windows.Data;

namespace ACCManager.Controls.Telemetry.RaceSessions
{
    internal class DivideBy1000ToFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                float result = (int)value / 1000f;
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
