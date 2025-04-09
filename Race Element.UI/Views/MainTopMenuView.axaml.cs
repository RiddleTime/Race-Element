using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RaceElement.UI.Views;

public partial class MainTopMenuView : UserControl
{
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public MainTopMenuView()
    {
        InitializeComponent();
    }
}