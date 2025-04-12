using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using RaceElement.UI.ViewModels;

namespace RaceElement.UI.Views.Pages;

public partial class SetupsPageView : ReactiveUserControl<SetupsViewModel>
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