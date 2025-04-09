using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RaceElement.UI.Views.Pages;

public partial class SetupsPageView : UserControl
{
  public SetupsPageView()
  {
    InitializeComponent();
  }
  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}