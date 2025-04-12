using Avalonia.Media;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace RaceElement.UI.ViewModels.OverlaySettingViewModels;

public class ControlButtonsPanelViewModel : ViewModelBase
{
    // Reference to the current overlay view model
    private ViewModelBase _currentOverlayViewModel;

    // Commands for each button in the control panel
    public ReactiveCommand<Unit, Unit> ActivateCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviewNowCommand { get; }
    public ReactiveCommand<Unit, Unit> AddFavoriteCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetToDefaultCommand { get; }
    public ReactiveCommand<Unit, Unit> MoveResizeCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; }

    public ControlButtonsPanelViewModel()
    {
        // Initialize commands with their implementations
        ActivateCommand = ReactiveCommand.CreateFromTask(OnActivateAsync);
        PreviewNowCommand = ReactiveCommand.CreateFromTask(OnPreviewNowAsync);
        AddFavoriteCommand = ReactiveCommand.CreateFromTask(OnAddFavoriteAsync);
        ResetToDefaultCommand = ReactiveCommand.CreateFromTask(OnResetToDefaultAsync);
        MoveResizeCommand = ReactiveCommand.CreateFromTask(OnMoveResizeAsync);
        SaveChangesCommand = ReactiveCommand.CreateFromTask(OnSaveChangesAsync);
    }

    // Property to determine the favorite icon's color
    private IBrush _favoriteIconColor = Brushes.White;
    public IBrush FavoriteIconColor
    {
        get => _favoriteIconColor;
        private set => this.RaiseAndSetIfChanged(ref _favoriteIconColor, value);
    }

    // Property to track if the current overlay is a favorite
    private bool _isCurrentOverlayFavorite;
    public bool IsCurrentOverlayFavorite
    {
        get => _isCurrentOverlayFavorite;
        set
        {
            this.RaiseAndSetIfChanged(ref _isCurrentOverlayFavorite, value);
            UpdateFavoriteIconColor();

            // If the current overlay is any type of OverlaySettingViewModelBase, update its IsFavorite property
            if (_currentOverlayViewModel is OverlaySettingViewModelBase accelerometerVM)
            {
                accelerometerVM.IsFavorite = value;
            }
        }
    }

    public void SetCurrentOverlayViewModel(ViewModelBase viewModel)
    {
        _currentOverlayViewModel = viewModel;

        // Update favorite status based on the current view model
        if (_currentOverlayViewModel is OverlaySettingViewModelBase accelerometerVM)
        {
            IsCurrentOverlayFavorite = accelerometerVM.IsFavorite;

            accelerometerVM.WhenAnyValue(vm => vm.IsFavorite)
                .Subscribe(isFavorite =>
                {
                    IsCurrentOverlayFavorite = isFavorite;
                });
        }
        else
        {
            IsCurrentOverlayFavorite = false;
        }
    }

    private void UpdateFavoriteIconColor()
    {
        FavoriteIconColor = IsCurrentOverlayFavorite ? Brushes.Red : Brushes.White;
    }

    private async Task OnActivateAsync()
    {
        // Implement the logic to activate the overlay
        await Task.CompletedTask;
    }

    private async Task OnPreviewNowAsync()
    {
        // Implement the logic to preview the overlay
        await Task.CompletedTask;
    }

    private async Task OnAddFavoriteAsync()
    {
        // Toggle the favorite status
        IsCurrentOverlayFavorite = !IsCurrentOverlayFavorite;
        await Task.CompletedTask;
    }

    private async Task OnResetToDefaultAsync()
    {
        // Implement the logic to reset settings to default values
        await Task.CompletedTask;
    }

    private async Task OnMoveResizeAsync()
    {
        // Implement the logic to reset settings to default values
        await Task.CompletedTask;
    }

    private async Task OnSaveChangesAsync()
    {
        // Implement the logic to save the current settings
        await Task.CompletedTask;
    }
}
