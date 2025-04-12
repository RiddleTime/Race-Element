using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using RaceElement.UI.ViewModels;

namespace RaceElement.UI.Views.Pages;

public partial class DataPageView : ReactiveUserControl<DataViewModel>
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