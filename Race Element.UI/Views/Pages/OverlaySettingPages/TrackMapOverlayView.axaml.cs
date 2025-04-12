using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using RaceElement.UI.ViewModels.OverlaySettingViewModels;
using RaceElement.UI.Views.OverlaySettingsPages;
using ReactiveUI;

namespace RaceElement.UI.Views.Pages.OverlaySettingPages;

public partial class TrackMapOverlayView : ReactiveUserControl<TrackMapOverlayViewModel>
{
    private ControlButtonsPanelView? _controlButtonsPanelView;
    public TrackMapOverlayView()
    {
        InitializeComponent();

        if (DataContext == null)
        {
            DataContext = new TrackMapOverlayViewModel();
        }

        this.WhenActivated(disposables =>
        {
            // Find the ControlButtonsPanelView
            _controlButtonsPanelView = this.FindControl<ControlButtonsPanelView>("ControlButtons");

            if (_controlButtonsPanelView != null && DataContext is TrackMapOverlayViewModel viewModel)
            {
                // Set the current overlay view model on the control buttons panel
                if (_controlButtonsPanelView.DataContext is ControlButtonsPanelViewModel controlButtonsVM)
                {
                    controlButtonsVM.SetCurrentOverlayViewModel(viewModel);
                }
            }
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // Find the control buttons panel once the view is loaded
        _controlButtonsPanelView = this.FindControl<ControlButtonsPanelView>("ControlButtons");

        if (_controlButtonsPanelView != null && DataContext is TrackMapOverlayViewModel viewModel)
        {
            // Set the current overlay view model on the control buttons panel
            if (_controlButtonsPanelView.DataContext is ControlButtonsPanelViewModel controlButtonsVM)
            {
                controlButtonsVM.SetCurrentOverlayViewModel(viewModel);
            }
        }
    }
}