using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace RaceElement.UI.Views.Stylers;

public sealed class ActiveButtonStyler : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    // Current page name from binding
    string currentPage = value as string;

    // Button's page name from parameter
    string buttonPage = parameter as string;

    // Check if this button matches the current page
    bool isSelected = currentPage == buttonPage;

    // Return appropriate value based on property type
    if (targetType == typeof(Thickness))
      return isSelected ? new Thickness(2) : new Thickness(1);

    if (targetType == typeof(IBrush))
      return isSelected ? new SolidColorBrush(Colors.DarkOrange) : new SolidColorBrush(Colors.Transparent);

    return null;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return value?.Equals(true) == true ? parameter : BindingOperations.DoNothing;
  }
}