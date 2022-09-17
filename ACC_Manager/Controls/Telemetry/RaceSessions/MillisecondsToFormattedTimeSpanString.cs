using System;
using System.Globalization;
using System.Windows.Data;

namespace ACCManager.Controls.Telemetry.RaceSessions
{
    internal class MillisecondsToFormattedTimeSpanString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                return $"{new TimeSpan(0, 0, 0, 0, (int)value):mm\\:ss\\:fff}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
