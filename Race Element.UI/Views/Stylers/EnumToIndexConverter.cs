using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

using RaceElement.UI.Services;

namespace RaceElement.UI.Views.Stylers;

public class EnumToIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OverlayType overlayType)
            return (int)overlayType;
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Check if the value is a valid index
        if (value is int index)
        {
            // Handle special case: -1 (no selection) should return a default value
            if (index == -1)
                return OverlayType.Inputs;

            // Check if it's a valid enum value
            if (Enum.IsDefined(typeof(OverlayType), index))
                return (OverlayType)index;
        }

        return OverlayType.Inputs;
    }
}
