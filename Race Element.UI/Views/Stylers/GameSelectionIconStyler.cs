using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.IO;

namespace RaceElement.UI.Views.Stylers;

public class GameSelectionIconStyler : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string imagePath)
        {
            try
            {
                // Handle asset paths
                if (imagePath.StartsWith("avares://"))
                {
                    var uri = new Uri(imagePath);

                    using var stream = AssetLoader.Open(uri);
                    return new Bitmap(stream);
                }

                // Handle local files
                return new Bitmap(imagePath);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}


