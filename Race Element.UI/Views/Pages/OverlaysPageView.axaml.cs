using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using RaceElement.UI.ViewModels;
using ReactiveUI;

namespace RaceElement.UI.Views.Pages;

public partial class OverlaysPageView : ReactiveUserControl<OverlaysViewModel>
{
    private ListBox OverlaysList => this.GetControl<ListBox>("OverlayItemsList");

    public OverlaysPageView()
    {
        InitializeComponent();

        // Set up additional bindings or event handlers in the WhenActivated method
        this.WhenActivated(disposables =>
        {
            // todo : Favorites can be checked
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
