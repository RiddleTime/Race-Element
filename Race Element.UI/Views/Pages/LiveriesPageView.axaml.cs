using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RaceElement.UI.Views.Pages;

public partial class LiveriesPageView : UserControl
{
  public LiveriesPageView()
  {
    InitializeComponent();
  }
  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}