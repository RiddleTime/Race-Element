using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using RaceElement.UI.ViewModels.OverlaySettingViewModels;
using ReactiveUI;

namespace RaceElement.UI.Views.OverlaySettingsPages;

public partial class ControlButtonsPanelView : ReactiveUserControl<ControlButtonsPanelViewModel>
{
    public ControlButtonsPanelView()
    {
        InitializeComponent();

        // Initialize ViewModel if not provided from parent
        if (DataContext == null)
        {
            DataContext = new ControlButtonsPanelViewModel();
        }

        this.WhenActivated(disposables =>
        {
            // Add any reactive disposables if needed
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
