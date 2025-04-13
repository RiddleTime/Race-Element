using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;

using RaceElement.UI.Services;
using RaceElement.UI.ViewModels.OverlaySettingViewModels;

namespace RaceElement.UI.ViewModels;

public class OverlaysViewModel : ViewModelBase
{
    #region Private Fields
    // Index of the currently selected overlay
    private int _selectedOverlayIndex;

    // Dictionary to hold game-specific view models
    private readonly Dictionary<string, Dictionary<int, ViewModelBase>> _gameSpecificViewModels = [];

    // The current game selected in the application
    private string _currentGame;
    #endregion

    public OverlaysViewModel()
    {
        // Initialize empty view model collections for each supported game
        _gameSpecificViewModels["iracing"] = new Dictionary<int, ViewModelBase>();
        _gameSpecificViewModels["acc"] = new Dictionary<int, ViewModelBase>();
        _gameSpecificViewModels["acevo"] = new Dictionary<int, ViewModelBase>();
        _gameSpecificViewModels["lmu"] = new Dictionary<int, ViewModelBase>();

        // Initialize with the current game from the service
        _currentGame = GameSelectionService.Instance.SelectedGame;

        // Subscribe to game changes
        GameSelectionService.Instance.GameChanged += OnGameChanged;

        // When the selected index changes, notify the CurrentOverlayViewModel property
        this.WhenAnyValue(x => x.SelectedOverlayIndex)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CurrentOverlayViewModel)));
    }

    #region Public Properties
    // Command to have additional logic
    public int SelectedOverlayIndex
    {
        get => _selectedOverlayIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedOverlayIndex, value);
    }

    public ViewModelBase CurrentOverlayViewModel => GetOrCreateViewModel(_selectedOverlayIndex);
    #endregion

    #region Private Methods

    //////////////////////////////////////////////////////////////////////////////////////////////
    ///                     HANDLE SELECTED GAME CHANGES                                       ///
    //////////////////////////////////////////////////////////////////////////////////////////////
    private void OnGameChanged(string newGame)
    {
        // Store current game
        _currentGame = newGame;

        // Force refresh of current view model
        this.RaisePropertyChanged(nameof(CurrentOverlayViewModel));
    }

    private ViewModelBase GetOrCreateViewModel(int index)
    {
        // Get the current game's view models
        var gameViewModels = _gameSpecificViewModels[_currentGame];

        // If we already have a view model for this index and game, return it
        if (gameViewModels.TryGetValue(index, out var existingViewModel))
        {
            return existingViewModel;
        }

        // Otherwise, create a new one
        var newViewModel = CreateViewModelForIndex(index);
        gameViewModels[index] = newViewModel;
        return newViewModel;
    }

    private ViewModelBase CreateViewModelForIndex(int index)
    {
        // Create the appropriate view model based on index
        return index switch
        {
            0 => new InputsOverlayViewModel(),
            1 => new StandingsOverlayViewModel(),
            2 => new RelativesOverlayViewModel(),
            3 => new LaptimeOverlayViewModel(),
            4 => new FuelOverlayViewModel(),
            5 => new TrackMapOverlayViewModel(),
            6 => new SpotterOverlayViewModel(),
            7 => new AccelerometerViewModel(),
            8 => new AverageLaptimeViewModel(),
            //9 => new BoostGaugeViewModel(),
            //10 => new BrakePressureViewModel(),
            _ => new InputsOverlayViewModel() // Default
        };
    }
    #endregion

    #region Cleanup
    public void Dispose()
    {
        // Unsubscribe from game changed event
        GameSelectionService.Instance.GameChanged -= OnGameChanged;
    }
    #endregion
}