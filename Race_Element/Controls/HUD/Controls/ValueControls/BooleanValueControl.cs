using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls;

internal sealed class BooleanValueControl : IValueControl<bool>, IControl
{
    private readonly Grid _grid;
    private readonly Label _label;
    private readonly ToggleButton _toggleButton;

    public FrameworkElement Control => _grid;
    public bool Value { get; set; }
    private readonly ConfigField _field;

    public BooleanValueControl(ConfigField configField)
    {
        _field = configField;
        _grid = new Grid()
        {
            Width = ControlConstants.ControlWidth,
            Margin = new Thickness(0, 0, 7, 0),
            Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
            Cursor = Cursors.Hand
        };
        _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
        _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10, GridUnitType.Star) });

        // add label
        _label = new Label()
        {
            HorizontalContentAlignment = HorizontalAlignment.Right,
            FontWeight = FontWeights.Bold
        };
        _grid.Children.Add(_label);
        Grid.SetColumn(_label, 0);

        // add toggle button
        _toggleButton = new ToggleButton()
        {
            IsChecked = bool.Parse(_field.Value.ToString())
        };
        _toggleButton.Checked += ToggleButtonValueChanged;
        _toggleButton.Unchecked += ToggleButtonValueChanged;
        UpdateLabel();
        _grid.Children.Add(_toggleButton);
        Grid.SetColumn(_toggleButton, 1);

        // add entire grid as toggle button decheck
        _grid.MouseLeftButtonUp += (s, e) =>
        {
            _toggleButton.IsChecked = !_toggleButton.IsChecked;
        };
    }

    private void ToggleButtonValueChanged(object sender, RoutedEventArgs e)
    {
        _field.Value = _toggleButton.IsChecked;
        UpdateLabel();
        Save();
    }

    private void UpdateLabel()
    {
        _label.Content = bool.Parse(_field.Value.ToString()) ? "On" : "Off";
    }

    public void Save()
    {
        ConfigurationControls.SaveOverlayConfigField(_field);
    }
}
