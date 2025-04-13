using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

using RaceElement.UI.ViewModels;
using RaceElement.UI.ViewModels.OverlaySettingViewModels;
using RaceElement.UI.Views.OverlaySettingsPages;

namespace RaceElement.UI.Views.Pages;

public partial class OverlaysPageView : ReactiveUserControl<OverlaysViewModel>
{
    private readonly ControlButtonsPanelView? _controlButtonsPanelView;
    private ListBox OverlaysList => this.GetControl<ListBox>("OverlayItemsList");

    public OverlaysPageView()
    {
        AvaloniaXamlLoader.Load(this);

        _controlButtonsPanelView = this.FindControl<ControlButtonsPanelView>("GlobalControlButtons");

        this.WhenActivated(disposables =>
        {
            // Listen for changes to the ViewModel property
            this.WhenAnyValue(x => x.ViewModel)
                .WhereNotNull()
                .Subscribe(viewModel =>
                {
                    // Initial setup when ViewModel is set
                    UpdateControlButtons(viewModel.SelectedOverlayIndex);

                    // Subscribe to changes in SelectedOverlayIndex
                    viewModel.WhenAnyValue(vm => vm.SelectedOverlayIndex)
                        .Subscribe(UpdateControlButtons)
                        .DisposeWith(disposables);
                })
                .DisposeWith(disposables);
        });
    }

    private void UpdateControlButtons(int index)
    {
        // Update the control buttons based on the selected overlay index
        if (_controlButtonsPanelView?.DataContext is ControlButtonsPanelViewModel controlButtonsVM &&
            ViewModel?.CurrentOverlayViewModel != null)
        {
            controlButtonsVM.SetCurrentOverlayViewModel(ViewModel.CurrentOverlayViewModel);
        }
    }
}
