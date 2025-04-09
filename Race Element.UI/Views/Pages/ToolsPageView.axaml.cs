using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RaceElement.UI.Views.Pages;

public partial class ToolsPageView : UserControl
{
  public ToolsPageView()
  {
    InitializeComponent();
  }
  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}