using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RaceElement.UI.Views.Pages;

public partial class DataPageView : UserControl
{
  public DataPageView()
  {
    InitializeComponent();
  }
  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}