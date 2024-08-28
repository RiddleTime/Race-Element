using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.Util.SystemExtensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls;

internal sealed class IntegerValueControl : IValueControl<int>, IControl
{
    private readonly Grid _grid;
    private readonly Label _label;
    private readonly Slider _slider;

    public FrameworkElement Control => _grid;
    public int Value { get; set; }
    private readonly ConfigField _field;

    public IntegerValueControl(IntRangeAttribute intRange, ConfigField configField)
    {
        _field = configField;
        _grid = new Grid()
        {
            Width = ControlConstants.ControlWidth,
            Margin = new Thickness(0, 0, 7, 0),
            Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
            Cursor = Cursors.Hand
        };
        _grid.PreviewMouseLeftButtonUp += (s, e) => Save();
        _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
        _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10, GridUnitType.Star) });

        _label = new Label()
        {
            Content = _field.Value,
            FontWeight = FontWeights.Bold,
            FontSize = 13,
        };
        _grid.Children.Add(_label);
        _label.HorizontalContentAlignment = HorizontalAlignment.Right;
        Grid.SetColumn(_label, 0);

        _slider = new Slider()
        {
            Minimum = intRange.GetMin(GameManager.CurrentGame),
            Maximum = intRange.GetMax(GameManager.CurrentGame),
            TickFrequency = intRange.Increment,
            IsSnapToTickEnabled = true,
            Width = 220,
        };
        _slider.ValueChanged += (s, e) =>
        {
            _field.Value = _slider.Value.ToString();
            _label.Content = _field.Value;
        };
        int value = int.Parse(_field.Value.ToString());
        value.Clip(intRange.GetMin(GameManager.CurrentGame), intRange.GetMin(GameManager.CurrentGame));
        _slider.Value = value;
        _grid.Children.Add(_slider);
        _slider.HorizontalAlignment = HorizontalAlignment.Right;
        _slider.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_slider, 1);

        _label.Content = _field.Value;
        Control.MouseWheel += (sender, args) =>
        {
            int delta = args.Delta;
            _slider.Value += delta.Clip(-1, 1) * intRange.Increment;
            args.Handled = true;
            Save();
        };
    }

    public void Save()
    {
        ConfigurationControls.SaveOverlayConfigField(_field);
    }
}
