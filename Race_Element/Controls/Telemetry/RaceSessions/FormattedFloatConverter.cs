using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RaceElement.Controls.Telemetry.RaceSessions;

internal class FormattedFloatConverter : IValueConverter
{
    private int _decimals;
    public FormattedFloatConverter(int decimals = 3)
    {
        this._decimals = decimals;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is float)
            return ((float)value).ToString($"F{_decimals}");

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
