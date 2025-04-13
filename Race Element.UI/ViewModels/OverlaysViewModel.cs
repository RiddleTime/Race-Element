using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;

using RaceElement.UI.Services;
using RaceElement.UI.ViewModels.OverlaySettingViewModels;
using System.Collections.ObjectModel;
using Material.Icons;
using System.Linq;

namespace RaceElement.UI.ViewModels;

public class OverlaysViewModel : ViewModelBase
{
    #region Private Fields
    // Index of the currently selected overlay
    private OverlayType _selectedOverlayType = OverlayType.Inputs;
    private OverlaySettingViewModelBase _selectedOverlayItem;

    // Dictionary to hold game-specific view models
    private readonly Dictionary<string, Dictionary<OverlayType, ViewModelBase>> _gameSpecificViewModels = [];

    // The current game selected in the application
    private string _currentGame = "acc";
    #endregion

    public OverlaysViewModel()
    {
        // Initialize empty view model collections for each supported game
        _gameSpecificViewModels["iracing"] = new Dictionary<OverlayType, ViewModelBase>();
        _gameSpecificViewModels["acc"] = new Dictionary<OverlayType, ViewModelBase>();
        _gameSpecificViewModels["acevo"] = new Dictionary<OverlayType, ViewModelBase>();
        _gameSpecificViewModels["lmu"] = new Dictionary<OverlayType, ViewModelBase>();

        // Initialize with the current game from the service
        _currentGame = GameSelectionService.Instance.SelectedGame;

        // Initialize available overlays
        UpdateAvailableOverlays();

        // Initialize the selected item
        _selectedOverlayItem = AvailableOverlays.First();
        _selectedOverlayType = _selectedOverlayItem.Type;

        // Subscribe to game changes
        GameSelectionService.Instance.GameChanged += OnGameChanged;

        // When the selected overlay menu changes, notify the CurrentOverlayViewModel property
        this.WhenAnyValue(x => x._selectedOverlayType)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CurrentOverlayViewModel)));
    }

    #region Public Properties
    public OverlayType SelectedOverlayType
    {
        get => _selectedOverlayType;
        set
        {
            // Validate the enum value before setting it in case overlay type is not available
            // and unsuported overlay type is selected for a game switch
            if (Enum.IsDefined(typeof(OverlayType), value))
            {
                this.RaiseAndSetIfChanged(ref _selectedOverlayType, value);
            }
            else
            {
                // If invalid, set to a default value
                this.RaiseAndSetIfChanged(ref _selectedOverlayType, OverlayType.Inputs);
            }
        }
    }

    public OverlaySettingViewModelBase SelectedOverlayItem
    {
        get => _selectedOverlayItem;
        set
        {
            if (value != null)
            {
                this.RaiseAndSetIfChanged(ref _selectedOverlayItem, value);
                // Update the SelectedOverlayType when the item changes
                SelectedOverlayType = value.Type;
            }
        }
    }

    public ViewModelBase CurrentOverlayViewModel => GetOrCreateViewModel(_selectedOverlayType);

    public ObservableCollection<OverlaySettingViewModelBase> AvailableOverlays { get; } = [];

    #endregion

    #region Private Methods
    private void UpdateAvailableOverlays()
    {
        AvailableOverlays.Clear();

        foreach (var overlayType in OverlaySelectionService.Instance.GetAvailableOverlaysForGame(_currentGame))
        {
            AvailableOverlays.Add(new OverlaySettingViewModelBase
            {
                Type = overlayType,
                Name = OverlaySelectionService.Instance.GetMenuNameForOverlayType(overlayType),
                IconKind = OverlaySelectionService.Instance.GetIconForOverlayType(overlayType)
            });
        }
    }

    /// <summary>
    /// Checks if the overlay is available for the current game
    /// </summary>
    /// <param name="overlayType"></param>
    /// <returns></returns>
    private bool IsOverlayAvailable(OverlayType overlayType)
    {
        return OverlaySelectionService.Instance.IsOverlayAvailableForGame(overlayType, _currentGame);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    ///                     HANDLE SELECTED GAME CHANGES                                       ///
    //////////////////////////////////////////////////////////////////////////////////////////////
    private void OnGameChanged(string newGame)
    {
        // Store current game
        _currentGame = newGame;

        // Update available overlays for the new game
        UpdateAvailableOverlays();

        // Force refresh of current view model
        this.RaisePropertyChanged(nameof(CurrentOverlayViewModel));

        // Make sure selected overlay is available in the new game
        // If not, select the first available overlay
        if (!IsOverlayAvailable(_selectedOverlayType))
        {
            OverlayType firstAvailable = OverlaySelectionService.Instance.FindFirstAvailableOverlay(_currentGame);
            SelectedOverlayType = firstAvailable;

            // Find the corresponding item in the available overlays
            SelectedOverlayItem = AvailableOverlays.FirstOrDefault(o => o.Type == firstAvailable) ??
                                 (AvailableOverlays.Count > 0 ? AvailableOverlays[0] : null)!;
        }
        else
        {
            // If still available, find the item in the new collection
            SelectedOverlayItem = AvailableOverlays.FirstOrDefault(o => o.Type == _selectedOverlayType) ??
                                 (AvailableOverlays.Count > 0 ? AvailableOverlays[0] : null)!;
        }
    }

    private ViewModelBase GetOrCreateViewModel(OverlayType type)
    {
        // Get the current game's view models
        var gameViewModels = _gameSpecificViewModels[_currentGame];

        // If we already have a view model for this index and game, return it
        if (gameViewModels.TryGetValue(type, out var existingViewModel))
        {
            return existingViewModel;
        }

        // Otherwise, create a new one
        var newViewModel = CreateViewModelForIndex(type);
        gameViewModels[type] = newViewModel;
        return newViewModel;
    }

    private ViewModelBase CreateViewModelForIndex(OverlayType type)
    {
        // Create the appropriate view model based on index
        return type switch
        {
            OverlayType.Inputs => new InputsOverlayViewModel(),
            OverlayType.Standings => new StandingsOverlayViewModel(),
            OverlayType.Relatives => new RelativesOverlayViewModel(),
            OverlayType.Laptimes => new LaptimeOverlayViewModel(),
            OverlayType.Fuel => new FuelOverlayViewModel(),
            OverlayType.TrackMap => new TrackMapOverlayViewModel(),
            OverlayType.Spotter => new SpotterOverlayViewModel(),
            OverlayType.Accelerometer => new AccelerometerViewModel(),
            OverlayType.AverageLaptime => new AverageLaptimeViewModel(),
            OverlayType.BoostGauge => new BoostGaugeViewModel(),
            OverlayType.BrakePressure => new BrakePressureViewModel(),
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