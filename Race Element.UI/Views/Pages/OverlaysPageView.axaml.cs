using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RaceElement.UI.Views.Pages;

public partial class OverlaysPageView : UserControl
{
  public OverlaysPageView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}
