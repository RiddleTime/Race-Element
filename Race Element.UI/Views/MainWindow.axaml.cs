using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

using RaceElement.UI.Views.Stylers;

namespace RaceElement.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Style the main window and set the default size
        MainWindowStyler.Decorate(this);
        Application.Current?.SetValue(
            ThemeVariantScope.ActualThemeVariantProperty,
            ThemeVariant.Light);
    }
}
