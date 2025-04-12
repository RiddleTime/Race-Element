using Avalonia.Media;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace RaceElement.UI.ViewModels.OverlaySettingViewModels;
public class OverlaySettingViewModelBase : ViewModelBase
{
    private bool _isFavorite;
    public bool IsFavorite
    {
        get => _isFavorite;
        set => this.RaiseAndSetIfChanged(ref _isFavorite, value);
    }

    // Display Settings
    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    private double _size = 150;
    public double Size
    {
        get => _size;
        set => this.RaiseAndSetIfChanged(ref _size, value);
    }

    private double _opacity = 0.8;
    public double Opacity
    {
        get => _opacity;
        set => this.RaiseAndSetIfChanged(ref _opacity, value);
    }

    // Behavior Settings
    private double _sensitivity = 1.0;
    public double Sensitivity
    {
        get => _sensitivity;
        set => this.RaiseAndSetIfChanged(ref _sensitivity, value);
    }

    private double _smoothing = 3;
    public double Smoothing
    {
        get => _smoothing;
        set => this.RaiseAndSetIfChanged(ref _smoothing, value);
    }

    // Position Settings
    private int _selectedPositionIndex = 0;
    public int SelectedPositionIndex
    {
        get => _selectedPositionIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedPositionIndex, value);
    }

    public ObservableCollection<string> PositionPresets { get; } = new ObservableCollection<string>
        {
            "Center", "Top Left", "Top Right", "Bottom Left", "Bottom Right", "Custom"
        };

    private double _xPosition = 50;
    public double XPosition
    {
        get => _xPosition;
        set => this.RaiseAndSetIfChanged(ref _xPosition, value);
    }

    private double _yPosition = 50;
    public double YPosition
    {
        get => _yPosition;
        set => this.RaiseAndSetIfChanged(ref _yPosition, value);
    }

    private bool _alwaysOnTop = true;
    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        set => this.RaiseAndSetIfChanged(ref _alwaysOnTop, value);
    }

    // Appearance
    private Color _backgroundColor = Colors.Transparent;
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set => this.RaiseAndSetIfChanged(ref _backgroundColor, value);
    }

    private Color _indicatorColor = Colors.Red;
    public Color IndicatorColor
    {
        get => _indicatorColor;
        set => this.RaiseAndSetIfChanged(ref _indicatorColor, value);
    }

    private bool _showGridLines = true;
    public bool ShowGridLines
    {
        get => _showGridLines;
        set => this.RaiseAndSetIfChanged(ref _showGridLines, value);
    }
}
