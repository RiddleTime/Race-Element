using Avalonia.Media;
using Material.Icons;
using ReactiveUI;
using System.Collections.ObjectModel;

using RaceElement.UI.Services;

namespace RaceElement.UI.ViewModels.OverlaySettingViewModels;
public class OverlaySettingViewModelBase : ViewModelBase
{
    private bool _isFavorite = false;
    private OverlayType _type = OverlayType.Inputs;
    private string _name = "";
    private MaterialIconKind _iconKind = MaterialIconKind.QuestionMark;

    public OverlaySettingViewModelBase()
    {
    }

    /// <summary>
    /// Indicates if this overlay is available for the currently selected game
    /// </summary>
    public bool IsAvailable => OverlaySelectionService.Instance.IsOverlayAvailableForGame(
        Type, GameSelectionService.Instance.SelectedGame);

    /// <summary>
    /// Indicates if this overlay is a favorite
    /// </summary>
    public bool IsFavorite
    {
        get => _isFavorite;
        set => this.RaiseAndSetIfChanged(ref _isFavorite, value);
    }

    /// <summary>
    /// The type of overlay this item represents
    /// </summary>
    public OverlayType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Display name of the overlay
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// The Material Icons kind to use for this overlay
    /// </summary>
    public MaterialIconKind IconKind
    {
        get => _iconKind;
        set => this.RaiseAndSetIfChanged(ref _iconKind, value);
    }

    // Display Settings
    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }
}
