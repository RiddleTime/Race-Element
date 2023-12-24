using RaceElement.Broadcast;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RaceElement.Controls.Telemetry.RaceSessions;

internal class LapTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && value is LapType)
        {
            switch (value)
            {
                case LapType.Inlap: return "In";
                case LapType.Outlap: return "Out";
                default: return string.Empty;
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
