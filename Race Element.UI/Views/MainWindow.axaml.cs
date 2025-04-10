using Avalonia;
using Avalonia.Controls;
using RaceElement.UI.ViewModels;
using Avalonia.Input;
using Avalonia.Interactivity;

using RaceElement.UI.Views.Stylers;
using Avalonia.Styling;

namespace RaceElement.UI.Views;

public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();

    // Style the main window and set the default size
    MainWindowStyler.Decorate(this);
        Application.Current.SetValue(ThemeVariantScope.ActualThemeVariantProperty, ThemeVariant.Dark);
    }
}
