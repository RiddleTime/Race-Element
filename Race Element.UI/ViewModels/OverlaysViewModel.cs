using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using RaceElement.UI.ViewModels.OverlaySettingViewModels;
using RaceElement.UI.Views.Pages.OverlaySettingPages;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive;

namespace RaceElement.UI.ViewModels;

public class OverlaysViewModel : ViewModelBase
{
    private int _selectedOverlayIndex;
    public int SelectedOverlayIndex
    {
        get => _selectedOverlayIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedOverlayIndex, value);
    }

    // You could optionally add a command if you need more complex logic
    public ReactiveCommand<int, Unit> SelectOverlayCommand { get; }

    public OverlaysViewModel()
    {
        // Initialize command
        SelectOverlayCommand = ReactiveCommand.Create<int>(index =>
        {
            SelectedOverlayIndex = index;
            // Any additional logic needed when selection changes
        });
    }
}